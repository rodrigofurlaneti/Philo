using Philo.Domain.Enum;
using Philo.Domain.Primitives;
namespace Philo.Domain.Entities
{
    public class ConversationEvent : AggregateRoot
    {
        public long OrganizationId { get; set; }
        public long ConversationId { get; set; }
        public long ActorId { get; set; }
        public EventType EventType { get; set; }
        public string Details { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public bool IsActive { get; private set; }
    }
}
