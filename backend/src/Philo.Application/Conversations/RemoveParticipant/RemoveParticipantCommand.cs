using FluentValidation;
using MediatR;
using Philo.Domain.Entities;
using Philo.Domain.Enums;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Conversations.RemoveParticipant
{
    /// <summary>Não remove fisicamente: preenche left_at. O cliente titular não pode ser retirado.</summary>
    public sealed record RemoveParticipantCommand(long OrganizationId, long ConversationId, long UserId, long ActorId) : IRequest<Result>;

    public sealed class RemoveParticipantValidator : AbstractValidator<RemoveParticipantCommand>
    {
        public RemoveParticipantValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.UserId).GreaterThan(0);
            RuleFor(x => x.ActorId).GreaterThan(0);
        }
    }

    public sealed class RemoveParticipantHandler : IRequestHandler<RemoveParticipantCommand, Result>
    {
        private readonly IConversationRepository _conversations;
        private readonly IConversationParticipantRepository _participants;
        private readonly IConversationEventRepository _events;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveParticipantHandler(
            IConversationRepository conversations, IConversationParticipantRepository participants,
            IConversationEventRepository events, IUnitOfWork unitOfWork)
        {
            _conversations = conversations;
            _participants = participants;
            _events = events;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(RemoveParticipantCommand request, CancellationToken cancellationToken) =>
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var conversation = await _conversations.GetByIdForOrganizationAsync(request.OrganizationId, request.ConversationId, ct);
                if (conversation is null)
                    return Result.Failure(DomainErrors.Conversation.NotFound);

                if (conversation.CustomerId == request.UserId)
                    return Result.Failure(DomainErrors.Participant.CannotRemoveCustomer);

                var participant = await _participants.GetAsync(request.OrganizationId, request.ConversationId, request.UserId, ct);
                if (participant is null)
                    return Result.Failure(DomainErrors.Participant.NotFound);

                var leaveResult = participant.Leave();
                if (leaveResult.IsFailure)
                    return leaveResult;

                await _participants.UpdateAsync(participant, ct);

                var evt = ConversationEvent.Create(
                    request.OrganizationId, request.ConversationId, request.ActorId,
                    ConversationEventType.ParticipantLeft, $"{{\"userId\":{request.UserId}}}");
                await _events.AddAsync(evt, ct);

                return Result.Success();
            }, cancellationToken);
    }
}
