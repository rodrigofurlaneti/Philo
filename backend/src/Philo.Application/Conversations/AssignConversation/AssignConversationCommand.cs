using FluentValidation;
using MediatR;
using Philo.Domain.Entities;
using Philo.Domain.Enums;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Conversations.AssignConversation
{
    /// <summary>
    /// Atribui (ou transfere) o atendimento a um agente/admin. Garante que ele ingresse como
    /// participante e recebe recibos das mensagens ainda sem destinatário nenhum (queries.sql #6),
    /// sem nunca substituir recibos já existentes nem se auto-adicionar como remetente.
    /// </summary>
    public sealed record AssignConversationCommand(long OrganizationId, long ConversationId, long AgentId, long ActorId) : IRequest<Result>;

    public sealed class AssignConversationValidator : AbstractValidator<AssignConversationCommand>
    {
        public AssignConversationValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.AgentId).GreaterThan(0);
            RuleFor(x => x.ActorId).GreaterThan(0);
        }
    }

    public sealed class AssignConversationHandler : IRequestHandler<AssignConversationCommand, Result>
    {
        private readonly IConversationRepository _conversations;
        private readonly IOrganizationUserRepository _memberships;
        private readonly IConversationParticipantRepository _participants;
        private readonly IMessageRepository _messages;
        private readonly IMessageReceiptRepository _receipts;
        private readonly IConversationEventRepository _events;
        private readonly IUnitOfWork _unitOfWork;

        public AssignConversationHandler(
            IConversationRepository conversations, IOrganizationUserRepository memberships,
            IConversationParticipantRepository participants, IMessageRepository messages,
            IMessageReceiptRepository receipts, IConversationEventRepository events, IUnitOfWork unitOfWork)
        {
            _conversations = conversations;
            _memberships = memberships;
            _participants = participants;
            _messages = messages;
            _receipts = receipts;
            _events = events;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(AssignConversationCommand request, CancellationToken cancellationToken)
        {
            var agentMembership = await _memberships.GetAsync(request.OrganizationId, request.AgentId, cancellationToken);
            if (agentMembership is null || !agentMembership.IsActive)
                return Result.Failure(DomainErrors.Membership.NotFound);
            if (agentMembership.Role is not (Role.Agent or Role.Admin))
                return Result.Failure(DomainErrors.Conversation.AssigneeMustBeStaff);

            return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var conversation = await _conversations.GetByIdForOrganizationAsync(request.OrganizationId, request.ConversationId, ct);
                if (conversation is null)
                    return Result.Failure(DomainErrors.Conversation.NotFound);

                var wasUnassigned = conversation.AssignedTo is null;

                var assignResult = conversation.AssignTo(request.AgentId);
                if (assignResult.IsFailure)
                    return assignResult;

                await _conversations.UpdateAsync(conversation, ct);

                var existingParticipant = await _participants.GetAsync(request.OrganizationId, request.ConversationId, request.AgentId, ct);
                if (existingParticipant is null)
                {
                    var participantResult = ConversationParticipant.Create(request.OrganizationId, request.ConversationId, request.AgentId);
                    if (participantResult.IsFailure)
                        return Result.Failure(participantResult.Error);

                    await _participants.AddAsync(participantResult.Value, ct);
                }
                else if (!existingParticipant.IsActive)
                {
                    var rejoinResult = existingParticipant.Rejoin();
                    if (rejoinResult.IsFailure)
                        return rejoinResult;

                    await _participants.UpdateAsync(existingParticipant, ct);
                }

                if (wasUnassigned)
                {
                    var pending = await _messages.GetPendingWithoutRecipientsAsync(request.OrganizationId, request.ConversationId, request.AgentId, ct);
                    var newReceipts = pending.Select(m => MessageReceipt.Create(request.OrganizationId, request.ConversationId, m.Id, request.AgentId));
                    await _receipts.AddRangeAsync(newReceipts, ct);
                }

                var evt = ConversationEvent.Create(
                    request.OrganizationId, request.ConversationId, request.ActorId,
                    ConversationEventType.Assigned, $"{{\"agentId\":{request.AgentId}}}");
                await _events.AddAsync(evt, ct);

                return Result.Success();
            }, cancellationToken);
        }
    }
}
