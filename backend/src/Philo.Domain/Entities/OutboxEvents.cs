using Philo.Domain.Enum;
namespace Philo.Domain.Entities
{
    public class OutboxEvents
    {
        public long OrganizationId { get; set; }
        public long ConversationId { get; set; }
        public string MessageId { get; set; }
        public EventType EventType { get; set; }
        public DateTime AvailableAt { get; set; }
        public DateTime PublishedAt { get; set; }
        public int Attempts { get; set; }
    }
}
