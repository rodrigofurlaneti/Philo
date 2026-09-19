using FluentValidation;
using MediatR;
using Philo.Domain.Entities;
using Philo.Domain.Enums;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Conversations.AddParticipant
{
    /// <summary>
    /// Inclusão de participante: apenas funcionários autorizados (ActorRole agent/admin, garantido
    /// no controller). Um cliente não pode adicionar outro cliente.
    /// </summary>
    public sealed record AddParticipantCommand(long OrganizationId, long ConversationId, long UserId, long ActorId) : IRequest<Result>;

    public sealed class AddParticipantValidator : AbstractValidator<AddParticipantCommand>
    {
        public AddParticipantValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.UserId).GreaterThan(0);
            RuleFor(x => x.ActorId).GreaterThan(0);
        }
    }

    public sealed class AddParticipantHandler : IRequestHandler<AddParticipantCommand, Result>
    {
        private readonly IConversationRepository _conversations;
        private readonly IOrganizationUserRepository _memberships;
        private readonly IConversationParticipantRepository _participants;
        private readonly IConversationEventRepository _events;
        private readonly IUnitOfWork _unitOfWork;

        public AddParticipantHandler(
            IConversationRepository conversations, IOrganizationUserRepository memberships,
            IConversationParticipantRepository participants, IConversationEventRepository events, IUnitOfWork unitOfWork)
        {
            _conversations = conversations;
            _memberships = memberships;
            _participants = participants;
            _events = events;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(AddParticipantCommand request, CancellationToken cancellationToken)
        {
            var membership = await _memberships.GetAsync(request.OrganizationId, request.UserId, cancellationToken);
            if (membership is null || !membership.IsActive)
                return Result.Failure(DomainErrors.Membership.NotFound);
            if (membership.Role == Role.Customer)
                return Result.Failure(DomainErrors.Participant.InsufficientRole);

            return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var conversation = await _conversations.GetByIdForOrganizationAsync(request.OrganizationId, request.ConversationId, ct);
                if (conversation is null)
                    return Result.Failure(DomainErrors.Conversation.NotFound);

                var existing = await _participants.GetAsync(request.OrganizationId, request.ConversationId, request.UserId, ct);
                if (existing is not null)
                {
                    if (existing.IsActive)
                        return Result.Failure(DomainErrors.Participant.AlreadyActive);

                    var rejoin = existing.Rejoin();
                    if (rejoin.IsFailure)
                        return rejoin;

                    await _participants.UpdateAsync(existing, ct);
                }
                else
                {
                    var createResult = ConversationParticipant.Create(request.OrganizationId, request.ConversationId, request.UserId);
                    if (createResult.IsFailure)
                        return Result.Failure(createResult.Error);

                    await _participants.AddAsync(createResult.Value, ct);
                }

                var evt = ConversationEvent.Create(
                    request.OrganizationId, request.ConversationId, request.ActorId,
                    ConversationEventType.ParticipantJoined, $"{{\"userId\":{request.UserId}}}");
                await _events.AddAsync(evt, ct);

                return Result.Success();
            }, cancellationToken);
        }
    }
}
