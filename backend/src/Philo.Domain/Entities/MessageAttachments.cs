using Philo.Domain.Enum;
namespace Philo.Domain.Entities
{
    public class MessageAttachments : BaseEntity
    {
        public long OrganizationId { get; set; }
        public long ConversationId { get; set; }
        public long MessageId { get; set; }
        public string OriginalName { get; set; }
        public string MimeType { get; set; }
        public long FileSizeBytes { get; set; }
        public string StorageBucket { get; set; }
        public string StorageKey { get; set; }
        public string Sha256 { get; set; }
        public ScanStatus ScanStatus { get; set; }
    }
}
