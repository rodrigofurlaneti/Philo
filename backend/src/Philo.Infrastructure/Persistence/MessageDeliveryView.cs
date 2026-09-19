namespace Philo.Infrastructure.Persistence
{
    /// <summary>Modelo de leitura para a view vw_message_delivery (somente consulta).</summary>
    public sealed class MessageDeliveryView
    {
        public long OrganizationId { get; init; }
        public long ConversationId { get; init; }
        public long MessageId { get; init; }
        public int ExpectedRecipients { get; init; }
        public int DeliveredCount { get; init; }
        public int ReadCount { get; init; }
        public string DeliveryStatus { get; init; } = string.Empty;
    }
}
