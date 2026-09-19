using Philo.Domain.Enums;
using Philo.Domain.Primitives;

namespace Philo.Domain.Entities
{
    public sealed class Message : AggregateRoot
    {
        public long OrganizationId { get; private set; }
        public long ConversationId { get; private set; }
        public long SenderId { get; private set; }
        public Guid ClientMessageId { get; private set; }
        public MessageType MessageType { get; private set; }
        public string? Body { get; private set; }
        public long? ReplyToId { get; private set; }
        public DateTime SentAt { get; private set; }
        public DateTime? EditedAt { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public DateTime? ExpiresAt { get; private set; }

        private readonly List<MessageAttachment> _attachments = [];
        public IReadOnlyCollection<MessageAttachment> Attachments => _attachments.AsReadOnly();

        public Conversation? Conversation { get; private set; }
        public User? Sender { get; private set; }

        private Message() : base(0) { }

        private Message(
            long organizationId, long conversationId, long senderId, Guid clientMessageId,
            MessageType messageType, string? body, long? replyToId, DateTime? expiresAt)
            : base(0)
        {
            OrganizationId = organizationId;
            ConversationId = conversationId;
            SenderId = senderId;
            ClientMessageId = clientMessageId;
            MessageType = messageType;
            Body = body;
            ReplyToId = replyToId;
            SentAt = DateTime.UtcNow;
            ExpiresAt = expiresAt;
        }

        public static Result<Message> CreateText(
            long organizationId, long conversationId, long senderId, Guid clientMessageId,
            string body, long? replyToId, DateTime? expiresAt)
        {
            if (string.IsNullOrWhiteSpace(body))
                return Result.Failure<Message>(Domain.Errors.DomainErrors.Message.EmptyBody);
            if (expiresAt is not null && expiresAt <= DateTime.UtcNow)
                return Result.Failure<Message>(Error.Validation("Message.InvalidExpiry", "expires_at deve ser posterior ao envio."));

            return Result.Success(new Message(
                organizationId, conversationId, senderId, clientMessageId,
                MessageType.Text, body.Trim(), replyToId, expiresAt));
        }

        public static Result<Message> CreateAttachment(
            long organizationId, long conversationId, long senderId, Guid clientMessageId,
            string? caption, long? replyToId, DateTime? expiresAt)
        {
            if (expiresAt is not null && expiresAt <= DateTime.UtcNow)
                return Result.Failure<Message>(Error.Validation("Message.InvalidExpiry", "expires_at deve ser posterior ao envio."));

            return Result.Success(new Message(
                organizationId, conversationId, senderId, clientMessageId,
                MessageType.Attachment, string.IsNullOrWhiteSpace(caption) ? null : caption.Trim(), replyToId, expiresAt));
        }

        public void AttachFile(MessageAttachment attachment) => _attachments.Add(attachment);

        /// <summary>Mensagens do tipo attachment exigem ao menos um arquivo confirmado antes do COMMIT.</summary>
        public bool HasRequiredAttachment => MessageType != MessageType.Attachment || _attachments.Count > 0;

        public Result Edit(long editorId, string newBody)
        {
            if (DeletedAt is not null)
                return Result.Failure(Domain.Errors.DomainErrors.Message.AlreadyDeleted);
            if (editorId != SenderId)
                return Result.Failure(Domain.Errors.DomainErrors.Message.NotAuthor);
            if (string.IsNullOrWhiteSpace(newBody))
                return Result.Failure(Domain.Errors.DomainErrors.Message.EmptyBody);

            Body = newBody.Trim();
            EditedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Delete(long requesterId)
        {
            if (DeletedAt is not null)
                return Result.Failure(Domain.Errors.DomainErrors.Message.AlreadyDeleted);
            if (requesterId != SenderId)
                return Result.Failure(Domain.Errors.DomainErrors.Message.NotAuthor);

            Body = null;
            DeletedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public bool IsDeleted => DeletedAt is not null;
        public bool IsExpired(DateTime asOfUtc) => ExpiresAt is not null && ExpiresAt <= asOfUtc;
    }
}
