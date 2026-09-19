namespace Philo.Domain.Entities
{
    public class ConversationProduct
    {
        public long OrganizationId { get; set; }
        public long ConversationId { get; set; }
        public long ProductId { get; set; }
        public string ProductNameSnapshot { get; set; }
        public Conversation Conversation { get; set; }
        public Product Product { get; set; }
    }
}
