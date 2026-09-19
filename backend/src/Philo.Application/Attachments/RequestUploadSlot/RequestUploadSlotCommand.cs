using FluentValidation;
using MediatR;
using Philo.Application.Common.Interfaces;
using Philo.Application.Common.Services;
using Philo.Domain.Errors;
using Philo.Domain.Primitives;

namespace Philo.Application.Attachments.RequestUploadSlot
{
    public sealed record UploadSlotDto(string Bucket, string StorageKey, string UploadUrl, DateTime ExpiresAtUtc);

    /// <summary>Passo 1 do envio de anexo: gera uma URL temporária de upload direto ao armazenamento privado.</summary>
    public sealed record RequestUploadSlotCommand(long OrganizationId, long ConversationId, long RequesterId, string FileName) : IRequest<Result<UploadSlotDto>>;

    public sealed class RequestUploadSlotValidator : AbstractValidator<RequestUploadSlotCommand>
    {
        public RequestUploadSlotValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.RequesterId).GreaterThan(0);
            RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        }
    }

    public sealed class RequestUploadSlotHandler : IRequestHandler<RequestUploadSlotCommand, Result<UploadSlotDto>>
    {
        private const string Bucket = "philo-attachments";

        private readonly IParticipantAccessGuard _accessGuard;
        private readonly IFileStorageService _storage;

        public RequestUploadSlotHandler(IParticipantAccessGuard accessGuard, IFileStorageService storage)
        {
            _accessGuard = accessGuard;
            _storage = storage;
        }

        public async Task<Result<UploadSlotDto>> Handle(RequestUploadSlotCommand request, CancellationToken cancellationToken)
        {
            var access = await _accessGuard.EnsureActiveParticipantAsync(request.OrganizationId, request.ConversationId, request.RequesterId, cancellationToken);
            if (access.IsFailure)
                return Result.Failure<UploadSlotDto>(access.Error);

            if (access.Value.Conversation.IsClosed)
                return Result.Failure<UploadSlotDto>(DomainErrors.Message.ConversationClosed);

            var slot = await _storage.CreateUploadSlotAsync(Bucket, request.FileName, cancellationToken);
            return Result.Success(new UploadSlotDto(slot.Bucket, slot.StorageKey, slot.UploadUrl, slot.ExpiresAtUtc));
        }
    }
}
