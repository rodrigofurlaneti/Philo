using Microsoft.EntityFrameworkCore;
using Philo.Domain.Entities;
using Philo.Domain.Interfaces;

namespace Philo.Infrastructure.Persistence.Repositories
{
    public sealed class MessageUserPreferenceRepository : IMessageUserPreferenceRepository
    {
        private readonly PhiloDbContext _context;

        public MessageUserPreferenceRepository(PhiloDbContext context) => _context = context;

        public async Task<MessageUserPreference?> GetAsync(long organizationId, long conversationId, long messageId, long userId, CancellationToken cancellationToken = default) =>
            await _context.MessageUserPreferences.FirstOrDefaultAsync(
                p => p.OrganizationId == organizationId && p.ConversationId == conversationId && p.MessageId == messageId && p.UserId == userId,
                cancellationToken);

        public async Task<IReadOnlyList<MessageUserPreference>> GetStarredAsync(long organizationId, long userId, int limit, CancellationToken cancellationToken = default) =>
            await _context.MessageUserPreferences
                .Where(p => p.OrganizationId == organizationId && p.UserId == userId && p.StarredAt != null)
                .OrderByDescending(p => p.StarredAt)
                .Take(limit)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(MessageUserPreference preference, CancellationToken cancellationToken = default)
        {
            await _context.MessageUserPreferences.AddAsync(preference, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(MessageUserPreference preference, CancellationToken cancellationToken = default)
        {
            _context.MessageUserPreferences.Update(preference);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
