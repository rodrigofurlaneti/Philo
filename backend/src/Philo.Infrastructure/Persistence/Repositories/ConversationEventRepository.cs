using Microsoft.EntityFrameworkCore;
using Philo.Domain.Entities;
using Philo.Domain.Interfaces;

namespace Philo.Infrastructure.Persistence.Repositories
{
    public sealed class ConversationEventRepository : IConversationEventRepository
    {
        private readonly PhiloDbContext _context;

        public ConversationEventRepository(PhiloDbContext context) => _context = context;

        public async Task AddAsync(ConversationEvent conversationEvent, CancellationToken cancellationToken = default)
        {
            await _context.ConversationEvents.AddAsync(conversationEvent, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ConversationEvent>> GetHistoryAsync(long organizationId, long conversationId, int limit, CancellationToken cancellationToken = default) =>
            await _context.ConversationEvents
                .Where(e => e.OrganizationId == organizationId && e.ConversationId == conversationId)
                .OrderByDescending(e => e.CreatedAt).ThenByDescending(e => e.Id)
                .Take(limit)
                .ToListAsync(cancellationToken);
    }
}
