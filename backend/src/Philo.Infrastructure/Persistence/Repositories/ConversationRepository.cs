using Microsoft.EntityFrameworkCore;
using Philo.Domain.Entities;
using Philo.Domain.Interfaces;

namespace Philo.Infrastructure.Persistence.Repositories
{
    public sealed class ConversationRepository : BaseRepository<Conversation>, IConversationRepository
    {
        public ConversationRepository(PhiloDbContext context) : base(context) { }

        public async Task<Conversation?> GetByIdForOrganizationAsync(long organizationId, long conversationId, CancellationToken cancellationToken = default) =>
            await Set.FirstOrDefaultAsync(c => c.OrganizationId == organizationId && c.Id == conversationId, cancellationToken);

        public async Task<Conversation?> GetWithParticipantsAsync(long organizationId, long conversationId, CancellationToken cancellationToken = default) =>
            await Set.Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId && c.Id == conversationId, cancellationToken);

        public async Task<IReadOnlyList<Conversation>> GetQueueAsync(long organizationId, string? status, int limit, CancellationToken cancellationToken = default)
        {
            var query = Set.Where(c => c.OrganizationId == organizationId);

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<Domain.Enums.ConversationStatus>(status, ignoreCase: true, out var parsedStatus))
                query = query.Where(c => c.Status == parsedStatus);

            return await query
                .OrderByDescending(c => c.LastActivityAt).ThenByDescending(c => c.Id)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Conversation>> GetForCustomerAsync(long organizationId, long customerId, int limit, CancellationToken cancellationToken = default) =>
            await Set.Where(c => c.OrganizationId == organizationId && c.CustomerId == customerId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
    }
}
