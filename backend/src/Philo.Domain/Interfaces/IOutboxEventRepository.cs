using Philo.Domain.Entities;

namespace Philo.Domain.Interfaces
{
    public interface IOutboxEventRepository
    {
        Task AddAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<OutboxEvent>> GetPendingBatchAsync(int batchSize, CancellationToken cancellationToken = default);
        Task UpdateAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default);
    }
}
