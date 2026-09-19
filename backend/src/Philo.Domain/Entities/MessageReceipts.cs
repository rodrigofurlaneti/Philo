namespace Philo.Domain.Entities
{
    public class MessageReceipts
    {
        public long OrganizationId { get; set; }
        public long ConversationId { get; set; }
        public long MessageId { get; set; }
        public long UserId { get; set; }
        public DateTime DeliveredAt { get; set; }
        public DateTime ReadAt { get; set; }
    }
}
