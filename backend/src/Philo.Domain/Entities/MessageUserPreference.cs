namespace Philo.Domain.Entities
{
    /// <summary>Preferência individual (organization_id, conversation_id, message_id, user_id): favorito/oculto.</summary>
    public sealed class MessageUserPreference
    {
        public long OrganizationId { get; private set; }
        public long ConversationId { get; private set; }
        public long MessageId { get; private set; }
        public long UserId { get; private set; }
        public DateTime? StarredAt { get; private set; }
        public DateTime? HiddenAt { get; private set; }

        private MessageUserPreference() { }

        private MessageUserPreference(long organizationId, long conversationId, long messageId, long userId)
        {
            OrganizationId = organizationId;
            ConversationId = conversationId;
            MessageId = messageId;
            UserId = userId;
        }

        public static MessageUserPreference Create(long organizationId, long conversationId, long messageId, long userId) =>
            new(organizationId, conversationId, messageId, userId);

        public void Star() => StarredAt = DateTime.UtcNow;
        public void Unstar() => StarredAt = null;
        public void Hide() => HiddenAt = DateTime.UtcNow;
        public void Unhide() => HiddenAt = null;
    }
}
