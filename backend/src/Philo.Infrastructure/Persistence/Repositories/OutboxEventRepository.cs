using Microsoft.EntityFrameworkCore;
using Philo.Domain.Entities;
using Philo.Domain.Interfaces;

namespace Philo.Infrastructure.Persistence.Repositories
{
    public sealed class OutboxEventRepository : IOutboxEventRepository
    {
        private readonly PhiloDbContext _context;

        public OutboxEventRepository(PhiloDbContext context) => _context = context;

        public async Task AddAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default)
        {
            await _context.OutboxEvents.AddAsync(outboxEvent, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<OutboxEvent>> GetPendingBatchAsync(int batchSize, CancellationToken cancellationToken = default)
        {
            var utcNow = DateTime.UtcNow;
            return await _context.OutboxEvents
                .Where(o => o.PublishedAt == null && o.AvailableAt <= utcNow)
                .OrderBy(o => o.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default)
        {
            _context.OutboxEvents.Update(outboxEvent);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
