using Philo.Domain.Entities;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Common.Services
{
    public sealed class ParticipantAccessGuard : IParticipantAccessGuard
    {
        private readonly IConversationRepository _conversations;
        private readonly IConversationParticipantRepository _participants;

        public ParticipantAccessGuard(IConversationRepository conversations, IConversationParticipantRepository participants)
        {
            _conversations = conversations;
            _participants = participants;
        }

        public async Task<Result<(Conversation Conversation, ConversationParticipant Participant)>> EnsureActiveParticipantAsync(
            long organizationId, long conversationId, long userId, CancellationToken cancellationToken = default)
        {
            var conversation = await _conversations.GetByIdForOrganizationAsync(organizationId, conversationId, cancellationToken);
            if (conversation is null)
                return Result.Failure<(Conversation, ConversationParticipant)>(DomainErrors.Conversation.NotFound);

            var participant = await _participants.GetAsync(organizationId, conversationId, userId, cancellationToken);
            if (participant is null || !participant.IsActive)
                return Result.Failure<(Conversation, ConversationParticipant)>(DomainErrors.Participant.NotActive);

            return Result.Success((conversation, participant));
        }
    }
}
