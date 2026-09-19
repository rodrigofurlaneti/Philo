namespace Philo.Domain.Entities
{
    public class ConversationParticipant
    {
        public long OrganizationId { get; set; }
        public long ConversationId { get; set; }
        public long UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime LeftAt { get; set; }
        public DateTime ArchivedAt { get; set; }
        public DateTime PinnedAt { get; set; }
        public DateTime MutedUntil { get; set; }
        public Conversation Conversation { get; set; }
        public User User { get; set; }
    }
}
