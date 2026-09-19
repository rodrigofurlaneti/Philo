using Microsoft.EntityFrameworkCore;
using Philo.Domain.Entities;
using Philo.Domain.Interfaces;

namespace Philo.Infrastructure.Persistence.Repositories
{
    public sealed class ConversationParticipantRepository : IConversationParticipantRepository
    {
        private readonly PhiloDbContext _context;

        public ConversationParticipantRepository(PhiloDbContext context) => _context = context;

        public async Task<ConversationParticipant?> GetAsync(long organizationId, long conversationId, long userId, CancellationToken cancellationToken = default) =>
            await _context.ConversationParticipants.FirstOrDefaultAsync(
                p => p.OrganizationId == organizationId && p.ConversationId == conversationId && p.UserId == userId, cancellationToken);

        public async Task<IReadOnlyList<ConversationParticipant>> GetActiveAsync(long organizationId, long conversationId, CancellationToken cancellationToken = default) =>
            await _context.ConversationParticipants
                .Where(p => p.OrganizationId == organizationId && p.ConversationId == conversationId && p.LeftAt == null)
                .ToListAsync(cancellationToken);

        public async Task<bool> IsActiveParticipantAsync(long organizationId, long conversationId, long userId, CancellationToken cancellationToken = default) =>
            await _context.ConversationParticipants.AnyAsync(
                p => p.OrganizationId == organizationId && p.ConversationId == conversationId && p.UserId == userId && p.LeftAt == null,
                cancellationToken);

        public async Task AddAsync(ConversationParticipant participant, CancellationToken cancellationToken = default)
        {
            await _context.ConversationParticipants.AddAsync(participant, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(ConversationParticipant participant, CancellationToken cancellationToken = default)
        {
            _context.ConversationParticipants.Update(participant);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
