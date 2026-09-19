using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Philo.Application.Common.Interfaces;
using Philo.Domain.Interfaces;

namespace Philo.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Publica os eventos já persistidos em outbox_events (message.created, etc.) para os
    /// clientes conectados. A trigger trg_message_created grava o registro na mesma transação
    /// da mensagem; este worker só entrega — se cair após publicar e antes de marcar published_at,
    /// o evento pode repetir e o consumidor deve deduplicar pelo id (validation.md).
    /// </summary>
    public sealed class OutboxPublisherService : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
        private const int BatchSize = 100;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxPublisherService> _logger;

        public OutboxPublisherService(IServiceScopeFactory scopeFactory, ILogger<OutboxPublisherService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PublishPendingBatchAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Falha ao publicar eventos da outbox.");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }

        private async Task PublishPendingBatchAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var outbox = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();
            var notifier = scope.ServiceProvider.GetRequiredService<IRealtimeNotifier>();

            var pending = await outbox.GetPendingBatchAsync(BatchSize, cancellationToken);
            foreach (var evt in pending)
            {
                await notifier.NotifyConversationAsync(
                    evt.OrganizationId, evt.ConversationId, evt.EventType,
                    new { evt.Id, evt.MessageId, evt.CreatedAt }, cancellationToken);

                evt.MarkPublished();
                evt.RegisterAttempt();
                await outbox.UpdateAsync(evt, cancellationToken);
            }
        }
    }
}
