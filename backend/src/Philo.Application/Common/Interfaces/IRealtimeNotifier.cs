namespace Philo.Application.Common.Interfaces
{
    /// <summary>Publica eventos já persistidos na outbox para os clientes conectados (ex.: SignalR).</summary>
    public interface IRealtimeNotifier
    {
        Task NotifyConversationAsync(long organizationId, long conversationId, string eventType, object payload, CancellationToken cancellationToken = default);
    }
}
