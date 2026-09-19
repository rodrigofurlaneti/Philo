using Philo.Domain.Primitives;

namespace Philo.Domain.Entities
{
    /// <summary>Recibo individual (organization_id, conversation_id, message_id, user_id).</summary>
    public sealed class MessageReceipt
    {
        public long OrganizationId { get; private set; }
        public long ConversationId { get; private set; }
        public long MessageId { get; private set; }
        public long UserId { get; private set; }
        public DateTime? DeliveredAt { get; private set; }
        public DateTime? ReadAt { get; private set; }

        private MessageReceipt() { }

        private MessageReceipt(long organizationId, long conversationId, long messageId, long userId)
        {
            OrganizationId = organizationId;
            ConversationId = conversationId;
            MessageId = messageId;
            UserId = userId;
        }

        public static MessageReceipt Create(long organizationId, long conversationId, long messageId, long userId) =>
            new(organizationId, conversationId, messageId, userId);

        public void MarkDelivered(DateTime? atUtc = null) => DeliveredAt ??= atUtc ?? DateTime.UtcNow;

        public Result MarkRead(DateTime? atUtc = null)
        {
            var when = atUtc ?? DateTime.UtcNow;
            DeliveredAt ??= when;
            if (when < DeliveredAt)
                return Result.Failure(Domain.Errors.DomainErrors.Receipt.ReadRequiresDelivery);

            ReadAt ??= when;
            return Result.Success();
        }

        public bool IsRead => ReadAt is not null;
        public bool IsDelivered => DeliveredAt is not null;
    }
}
