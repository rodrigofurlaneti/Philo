using Philo.Domain.Enum;
namespace Philo.Domain.Entities
{
    public class Message : BaseEntity
    {
        public long OrganizationId { get; set; }
        public long ConversationId { get; set; }
        public long SenderId { get; set; }
        public long ClientMessageId { get; set; }
        public MessageType MessageType { get; set; }
        public string Body { get; set; }
        public string ReplayToId { get; set; }
        public DateTime DeletedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public Conversation Conversation { get; set; }
        public User Sender { get; set; }
    }
}
