using Philo.Domain.Enums;
using Philo.Domain.Primitives;

namespace Philo.Domain.Entities
{
    public sealed class MessageAttachment : Entity
    {
        public const long MaxFileSizeBytes = 104_857_600; // 100 MiB

        public long OrganizationId { get; private set; }
        public long ConversationId { get; private set; }
        public long MessageId { get; private set; }
        public string OriginalName { get; private set; } = null!;
        public string MimeType { get; private set; } = null!;
        public long FileSizeBytes { get; private set; }
        public string StorageBucket { get; private set; } = null!;
        public string StorageKey { get; private set; } = null!;
        public byte[] Sha256 { get; private set; } = [];
        public ScanStatus ScanStatus { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private MessageAttachment() : base(0) { }

        private MessageAttachment(
            long organizationId, long conversationId, long messageId,
            string originalName, string mimeType, long fileSizeBytes,
            string storageBucket, string storageKey, byte[] sha256)
            : base(0)
        {
            OrganizationId = organizationId;
            ConversationId = conversationId;
            MessageId = messageId;
            OriginalName = originalName;
            MimeType = mimeType;
            FileSizeBytes = fileSizeBytes;
            StorageBucket = storageBucket;
            StorageKey = storageKey;
            Sha256 = sha256;
            ScanStatus = ScanStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<MessageAttachment> Create(
            long organizationId, long conversationId, long messageId,
            string originalName, string mimeType, long fileSizeBytes,
            string storageBucket, string storageKey, byte[] sha256)
        {
            if (fileSizeBytes <= 0 || fileSizeBytes > MaxFileSizeBytes)
                return Result.Failure<MessageAttachment>(Domain.Errors.DomainErrors.Attachment.InvalidSize);
            if (string.IsNullOrWhiteSpace(originalName))
                return Result.Failure<MessageAttachment>(Error.Validation("Attachment.NameRequired", "O nome original do arquivo é obrigatório."));
            if (string.IsNullOrWhiteSpace(mimeType))
                return Result.Failure<MessageAttachment>(Error.Validation("Attachment.MimeRequired", "O tipo do arquivo é obrigatório."));
            if (sha256.Length != 32)
                return Result.Failure<MessageAttachment>(Error.Validation("Attachment.InvalidHash", "O hash SHA-256 deve ter 32 bytes."));

            return Result.Success(new MessageAttachment(
                organizationId, conversationId, messageId, originalName, mimeType,
                fileSizeBytes, storageBucket, storageKey, sha256));
        }

        public void MarkClean() => ScanStatus = ScanStatus.Clean;
        public void Reject() => ScanStatus = ScanStatus.Rejected;
        public bool IsDownloadable => ScanStatus == ScanStatus.Clean;
    }
}
