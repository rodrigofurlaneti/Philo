using FluentValidation;
using MediatR;
using Philo.Application.Common.Interfaces;
using Philo.Application.Common.Services;
using Philo.Application.Messages.Common;
using Philo.Domain.Entities;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Messages.SendAttachmentMessage
{
    /// <summary>
    /// Envia mensagem do tipo attachment. Os arquivos já devem estar no storage privado
    /// (via slot obtido em Attachments/RequestUploadSlot). Mensagem e anexo(s) são gravados
    /// na mesma transação: sem nenhum anexo válido, tudo é revertido (README requisito 8).
    /// </summary>
    public sealed record SendAttachmentMessageCommand(
        long OrganizationId, long ConversationId, long SenderId, Guid ClientMessageId,
        string? Caption, long? ReplyToId, DateTime? ExpiresAt,
        IReadOnlyList<AttachmentInput> Attachments) : IRequest<Result<long>>;

    public sealed class SendAttachmentMessageValidator : AbstractValidator<SendAttachmentMessageCommand>
    {
        public SendAttachmentMessageValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.SenderId).GreaterThan(0);
            RuleFor(x => x.ClientMessageId).NotEmpty();
            RuleFor(x => x.Caption).MaximumLength(2000);
            RuleFor(x => x.Attachments).NotEmpty().WithMessage("Mensagens de anexo exigem ao menos um arquivo.");
            RuleForEach(x => x.Attachments).ChildRules(a =>
            {
                a.RuleFor(i => i.StorageKey).NotEmpty();
                a.RuleFor(i => i.OriginalName).NotEmpty().MaximumLength(255);
                a.RuleFor(i => i.MimeType).NotEmpty().MaximumLength(127);
            });
        }
    }

    public sealed class SendAttachmentMessageHandler : IRequestHandler<SendAttachmentMessageCommand, Result<long>>
    {
        private const string Bucket = "philo-attachments";

        private readonly IParticipantAccessGuard _accessGuard;
        private readonly IMessageRepository _messages;
        private readonly IMessageAttachmentRepository _attachments;
        private readonly IConversationRepository _conversations;
        private readonly IFileStorageService _storage;
        private readonly IAttachmentScanner _scanner;
        private readonly IUnitOfWork _unitOfWork;

        public SendAttachmentMessageHandler(
            IParticipantAccessGuard accessGuard, IMessageRepository messages, IMessageAttachmentRepository attachments,
            IConversationRepository conversations, IFileStorageService storage, IAttachmentScanner scanner, IUnitOfWork unitOfWork)
        {
            _accessGuard = accessGuard;
            _messages = messages;
            _attachments = attachments;
            _conversations = conversations;
            _storage = storage;
            _scanner = scanner;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<long>> Handle(SendAttachmentMessageCommand request, CancellationToken cancellationToken)
        {
            var existing = await _messages.GetByClientMessageIdAsync(
                request.OrganizationId, request.ConversationId, request.SenderId, request.ClientMessageId, cancellationToken);
            if (existing is not null)
                return Result.Success(existing.Id);

            var access = await _accessGuard.EnsureActiveParticipantAsync(request.OrganizationId, request.ConversationId, request.SenderId, cancellationToken);
            if (access.IsFailure)
                return Result.Failure<long>(access.Error);

            var (conversation, _) = access.Value;
            if (conversation.IsClosed)
                return Result.Failure<long>(DomainErrors.Message.ConversationClosed);

            return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var messageResult = Message.CreateAttachment(
                    request.OrganizationId, request.ConversationId, request.SenderId, request.ClientMessageId,
                    request.Caption, request.ReplyToId, request.ExpiresAt);
                if (messageResult.IsFailure)
                    return Result.Failure<long>(messageResult.Error);

                var message = messageResult.Value;
                await _messages.AddAsync(message, ct);

                foreach (var input in request.Attachments)
                {
                    var sizeBytes = await _storage.GetFileSizeAsync(Bucket, input.StorageKey, ct);
                    var sha256 = await _storage.ComputeSha256Async(Bucket, input.StorageKey, ct);

                    var attachmentResult = MessageAttachment.Create(
                        request.OrganizationId, request.ConversationId, message.Id,
                        input.OriginalName, input.MimeType, sizeBytes, Bucket, input.StorageKey, sha256);
                    if (attachmentResult.IsFailure)
                        return Result.Failure<long>(attachmentResult.Error);

                    var attachment = attachmentResult.Value;
                    var verdict = await _scanner.ScanAsync(input.MimeType, sizeBytes, sha256, ct);
                    if (verdict == ScanVerdict.Clean)
                        attachment.MarkClean();
                    else
                        attachment.Reject();

                    await _attachments.AddAsync(attachment, ct);
                    message.AttachFile(attachment);
                }

                if (!message.HasRequiredAttachment)
                    return Result.Failure<long>(DomainErrors.Message.AttachmentRequired);

                // Reflete de quem é a vez de responder (README não cobre; regra de produto da UI da fila).
                conversation.RegisterMessageFrom(request.SenderId);
                await _conversations.UpdateAsync(conversation, ct);

                return Result.Success(message.Id);
            }, cancellationToken);
        }
    }
}
