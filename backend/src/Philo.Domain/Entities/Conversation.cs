using Philo.Domain.Enum;
using Philo.Domain.Primitives;
namespace Philo.Domain.Entities
{
    public sealed class Conversation : AggregateRoot
    {
        public long OrganizationId { get; private set; }
        public long CustomerId { get; private set; }
        public long? AssignedId { get; private set; }
        public string Subject { get; private set; } = null!;
        public ConversationStatus ConversationStatus { get; private set; }
        public Priority Priority { get; private set; }
        public string SourcePageUrl { get; private set; } = null!;
        public DateTime? LastActivityAt { get; private set; }
        public DateTime? ClosedAt { get; private set; }
        public Organization Organization { get; private set; }
        public User Assigned { get; private set; }
        public DateTime? CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public bool IsActive { get; private set; }
        public ICollection<ConversationParticipant> ConversationParticipants { get; private set; }
        public ICollection<Message> Messages { get; private set; }
        public ICollection<ConversationEvent> ConversationEvents { get; private set; }
        public Conversation() : base(0) {}

        public Conversation(long organizationId, long customerId, long? assignedId, string subject, ConversationStatus conversationStatus, Priority priority, string sourcePageUrl, DateTime? lastActivityAt, DateTime? closedAt, Organization organization, User assigned, DateTime? createdAt, DateTime? updatedAt, bool isActive, ICollection<ConversationParticipant> conversationParticipants, ICollection<Message> messages, ICollection<ConversationEvent> conversationEvents)
        {
            OrganizationId = organizationId;
            CustomerId = customerId;
            AssignedId = assignedId;
            Subject = subject;
            ConversationStatus = conversationStatus;
            Priority = priority;
            SourcePageUrl = sourcePageUrl;
            LastActivityAt = lastActivityAt;
            ClosedAt = closedAt;
            Organization = organization;
            Assigned = assigned;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            IsActive = isActive;
            ConversationParticipants = conversationParticipants;
            Messages = messages;
            ConversationEvents = conversationEvents;
        }
    }
}
