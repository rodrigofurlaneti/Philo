using FluentValidation;
using MediatR;
using Philo.Application.Common.Interfaces;
using Philo.Application.Common.Services;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Attachments.GetDownloadUrl
{
    /// <summary>
    /// Autoriza o acesso e só então gera a URL temporária: anexos pending/rejected ou de mensagens
    /// apagadas/expiradas/ocultadas nunca são liberados (README/validation.md).
    /// </summary>
    public sealed record GetDownloadUrlQuery(long OrganizationId, long ConversationId, long AttachmentId, long RequesterId) : IRequest<Result<string>>;

    public sealed class GetDownloadUrlValidator : AbstractValidator<GetDownloadUrlQuery>
    {
        public GetDownloadUrlValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.AttachmentId).GreaterThan(0);
            RuleFor(x => x.RequesterId).GreaterThan(0);
        }
    }

    public sealed class GetDownloadUrlHandler : IRequestHandler<GetDownloadUrlQuery, Result<string>>
    {
        private readonly IParticipantAccessGuard _accessGuard;
        private readonly IMessageAttachmentRepository _attachments;
        private readonly IMessageRepository _messages;
        private readonly IFileStorageService _storage;

        public GetDownloadUrlHandler(
            IParticipantAccessGuard accessGuard, IMessageAttachmentRepository attachments,
            IMessageRepository messages, IFileStorageService storage)
        {
            _accessGuard = accessGuard;
            _attachments = attachments;
            _messages = messages;
            _storage = storage;
        }

        public async Task<Result<string>> Handle(GetDownloadUrlQuery request, CancellationToken cancellationToken)
        {
            var access = await _accessGuard.EnsureActiveParticipantAsync(request.OrganizationId, request.ConversationId, request.RequesterId, cancellationToken);
            if (access.IsFailure)
                return Result.Failure<string>(access.Error);

            var attachment = await _attachments.GetAsync(request.OrganizationId, request.ConversationId, request.AttachmentId, cancellationToken);
            if (attachment is null)
                return Result.Failure<string>(DomainErrors.Attachment.NotFound);

            if (!attachment.IsDownloadable)
                return Result.Failure<string>(attachment.ScanStatus == Domain.Enums.ScanStatus.Rejected
                    ? DomainErrors.Attachment.Rejected
                    : DomainErrors.Attachment.NotReady);

            var message = await _messages.GetByIdAsync(request.OrganizationId, request.ConversationId, attachment.MessageId, cancellationToken);
            if (message is null || message.IsDeleted || message.IsExpired(DateTime.UtcNow))
                return Result.Failure<string>(DomainErrors.Attachment.NotFound);

            var url = await _storage.CreateDownloadUrlAsync(attachment.StorageBucket, attachment.StorageKey, TimeSpan.FromMinutes(10), cancellationToken);
            return Result.Success(url);
        }
    }
}
