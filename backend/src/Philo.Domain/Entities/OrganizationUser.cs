using Philo.Domain.Enum;
namespace Philo.Domain.Entities
{
    public class OrganizationUser
    {
        public long OrganizationId { get; set; }
        public long UserId { get; set; }
        public Role Role { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Organization Organization { get; set; }
        public User User { get; set; }
        public ICollection<Conversation> Conversations { get; set; }
    }
}