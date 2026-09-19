using Philo.Domain.Primitives;

namespace Philo.Domain.Entities
{
    /// <summary>Fila transacional para WebSocket/notificações; não significa entrega ao usuário.</summary>
    public sealed class OutboxEvent : Entity
    {
        public long OrganizationId { get; private set; }
        public long ConversationId { get; private set; }
        public long? MessageId { get; private set; }
        public string EventType { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }
        public DateTime AvailableAt { get; private set; }
        public DateTime? PublishedAt { get; private set; }
        public uint Attempts { get; private set; }

        private OutboxEvent() : base(0) { }

        private OutboxEvent(long organizationId, long conversationId, long? messageId, string eventType, DateTime? availableAt)
            : base(0)
        {
            OrganizationId = organizationId;
            ConversationId = conversationId;
            MessageId = messageId;
            EventType = eventType;
            CreatedAt = DateTime.UtcNow;
            AvailableAt = availableAt ?? CreatedAt;
        }

        public static OutboxEvent Create(long organizationId, long conversationId, long? messageId, string eventType, DateTime? availableAt = null) =>
            new(organizationId, conversationId, messageId, eventType, availableAt);

        public void MarkPublished() => PublishedAt = DateTime.UtcNow;
        public void RegisterAttempt() => Attempts += 1;
        public bool IsPending => PublishedAt is null;
    }
}
