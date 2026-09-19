using Philo.Domain.Enums;
using Philo.Domain.Primitives;

namespace Philo.Domain.Entities
{
    /// <summary>Auditoria interna, somente por acréscimo. Não expor automaticamente ao cliente.</summary>
    public sealed class ConversationEvent : Entity
    {
        public long OrganizationId { get; private set; }
        public long ConversationId { get; private set; }
        public long? ActorId { get; private set; }
        public ConversationEventType EventType { get; private set; }
        public string Details { get; private set; } = "{}";
        public DateTime CreatedAt { get; private set; }

        private ConversationEvent() : base(0) { }

        private ConversationEvent(long organizationId, long conversationId, long? actorId, ConversationEventType eventType, string details)
            : base(0)
        {
            OrganizationId = organizationId;
            ConversationId = conversationId;
            ActorId = actorId;
            EventType = eventType;
            Details = details;
            CreatedAt = DateTime.UtcNow;
        }

        public static ConversationEvent Create(long organizationId, long conversationId, long? actorId, ConversationEventType eventType, string detailsJson) =>
            new(organizationId, conversationId, actorId, eventType, string.IsNullOrWhiteSpace(detailsJson) ? "{}" : detailsJson);
    }
}
