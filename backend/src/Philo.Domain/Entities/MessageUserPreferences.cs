namespace Philo.Domain.Entities
{
    public class MessageUserPreferences : BaseEntity
    {
        public long OrganizationId { get; set; }
        public long ConversationId { get; set; }
        public long UserId { get; set; }
        public long MessageId { get; set; }
        public DateTime StarredAt { get; set; }
        public DateTime HiddenAt { get; set; }
    }
}
