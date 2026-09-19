namespace Philo.Domain.Entities
{
    public class User : BaseEntity
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string PhoneE164 { get; set; }
        public string AvatarObjectKey { get; set; }
        public ICollection<OrganizationUser> OrganizationUsers { get; set; }
        public ICollection<ConversationParticipant> ConversationParticipants { get; set; }
    }
}
