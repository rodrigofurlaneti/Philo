using Microsoft.EntityFrameworkCore;
using Philo.Domain.Entities;
using Philo.Domain.Interfaces;

namespace Philo.Infrastructure.Persistence.Repositories
{
    public sealed class ConversationProductRepository : IConversationProductRepository
    {
        private readonly PhiloDbContext _context;

        public ConversationProductRepository(PhiloDbContext context) => _context = context;

        public async Task<IReadOnlyList<ConversationProduct>> GetAsync(long organizationId, long conversationId, CancellationToken cancellationToken = default) =>
            await _context.ConversationProducts
                .Where(cp => cp.OrganizationId == organizationId && cp.ConversationId == conversationId)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(ConversationProduct link, CancellationToken cancellationToken = default)
        {
            await _context.ConversationProducts.AddAsync(link, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
