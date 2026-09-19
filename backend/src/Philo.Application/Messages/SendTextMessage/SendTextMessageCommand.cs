using FluentValidation;
using MediatR;
using Philo.Application.Common.Services;
using Philo.Domain.Entities;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Messages.SendTextMessage
{
    /// <summary>
    /// Envia mensagem de texto. Idempotente por (organização, conversa, remetente, client_message_id):
    /// repetir a mesma tentativa retorna a mensagem já criada em vez de duplicar (README/validation.md).
    /// Recibos, atividade da conversa e evento de outbox são responsabilidade da trigger trg_message_created.
    /// </summary>
    public sealed record SendTextMessageCommand(
        long OrganizationId, long ConversationId, long SenderId, Guid ClientMessageId,
        string Body, long? ReplyToId, DateTime? ExpiresAt) : IRequest<Result<long>>;

    public sealed class SendTextMessageValidator : AbstractValidator<SendTextMessageCommand>
    {
        public SendTextMessageValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.SenderId).GreaterThan(0);
            RuleFor(x => x.ClientMessageId).NotEmpty();
            RuleFor(x => x.Body).NotEmpty().MaximumLength(8000);
        }
    }

    public sealed class SendTextMessageHandler : IRequestHandler<SendTextMessageCommand, Result<long>>
    {
        private readonly IParticipantAccessGuard _accessGuard;
        private readonly IMessageRepository _messages;
        private readonly IUnitOfWork _unitOfWork;

        public SendTextMessageHandler(IParticipantAccessGuard accessGuard, IMessageRepository messages, IUnitOfWork unitOfWork)
        {
            _accessGuard = accessGuard;
            _messages = messages;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<long>> Handle(SendTextMessageCommand request, CancellationToken cancellationToken)
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

            if (request.ReplyToId is not null)
            {
                var replyValidation = await ValidateReplyAsync(request.OrganizationId, request.ConversationId, request.ReplyToId.Value, cancellationToken);
                if (replyValidation.IsFailure)
                    return Result.Failure<long>(replyValidation.Error);
            }

            var messageResult = Message.CreateText(
                request.OrganizationId, request.ConversationId, request.SenderId, request.ClientMessageId,
                request.Body, request.ReplyToId, request.ExpiresAt);
            if (messageResult.IsFailure)
                return Result.Failure<long>(messageResult.Error);

            await _messages.AddAsync(messageResult.Value, cancellationToken);
            return Result.Success(messageResult.Value.Id);
        }

        internal async Task<Result> ValidateReplyAsync(long organizationId, long conversationId, long replyToId, CancellationToken cancellationToken)
        {
            var replied = await _messages.GetByIdAsync(organizationId, conversationId, replyToId, cancellationToken);
            if (replied is null || replied.IsDeleted || replied.IsExpired(DateTime.UtcNow))
                return Result.Failure(DomainErrors.Message.ReplyNotVisible);

            return Result.Success();
        }
    }
}
