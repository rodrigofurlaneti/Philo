namespace Philo.Domain.Entities
{
    public class Product : BaseEntity
    {
        public long OrganizationId { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string PageUrl { get; set; }
        public Organization Organization { get; set; }
        public ICollection<ConversationEvent> ConversationEvents { get; set; }
    }
}
