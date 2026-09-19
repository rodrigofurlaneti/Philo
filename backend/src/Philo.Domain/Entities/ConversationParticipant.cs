using Philo.Domain.Primitives;

namespace Philo.Domain.Entities
{
    /// <summary>Vínculo (organization_id, conversation_id, user_id) de participação em uma conversa.</summary>
    public sealed class ConversationParticipant
    {
        public long OrganizationId { get; private set; }
        public long ConversationId { get; private set; }
        public long UserId { get; private set; }
        public DateTime JoinedAt { get; private set; }
        public DateTime? LeftAt { get; private set; }
        public DateTime? ArchivedAt { get; private set; }
        public DateTime? PinnedAt { get; private set; }
        public DateTime? MutedUntil { get; private set; }

        public Conversation? Conversation { get; private set; }
        public User? User { get; private set; }

        private ConversationParticipant() { }

        private ConversationParticipant(long organizationId, long conversationId, long userId)
        {
            OrganizationId = organizationId;
            ConversationId = conversationId;
            UserId = userId;
            JoinedAt = DateTime.UtcNow;
        }

        public static Result<ConversationParticipant> Create(long organizationId, long conversationId, long userId)
        {
            if (organizationId <= 0 || conversationId < 0 || userId <= 0)
                return Result.Failure<ConversationParticipant>(Error.Validation("Participant.InvalidIds", "Identificadores inválidos para participante."));

            return Result.Success(new ConversationParticipant(organizationId, conversationId, userId));
        }

        public Result Rejoin()
        {
            if (LeftAt is null)
                return Result.Failure(Domain.Errors.DomainErrors.Participant.AlreadyActive);

            LeftAt = null;
            JoinedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Leave()
        {
            if (LeftAt is not null)
                return Result.Failure(Domain.Errors.DomainErrors.Participant.NotActive);

            LeftAt = DateTime.UtcNow;
            return Result.Success();
        }

        public void Archive() => ArchivedAt = DateTime.UtcNow;
        public void Unarchive() => ArchivedAt = null;
        public void Pin() => PinnedAt = DateTime.UtcNow;
        public void Unpin() => PinnedAt = null;
        public void Mute(DateTime until) => MutedUntil = until;
        public void Unmute() => MutedUntil = null;

        public bool IsActive => LeftAt is null;
    }
}
