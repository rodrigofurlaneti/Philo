using Microsoft.EntityFrameworkCore;
using Philo.Domain.Entities;
using Philo.Domain.Interfaces;

namespace Philo.Infrastructure.Persistence.Repositories
{
    public sealed class MessageReceiptRepository : IMessageReceiptRepository
    {
        private readonly PhiloDbContext _context;

        public MessageReceiptRepository(PhiloDbContext context) => _context = context;

        public async Task<MessageReceipt?> GetAsync(long organizationId, long conversationId, long messageId, long userId, CancellationToken cancellationToken = default) =>
            await _context.MessageReceipts.FirstOrDefaultAsync(
                r => r.OrganizationId == organizationId && r.ConversationId == conversationId && r.MessageId == messageId && r.UserId == userId,
                cancellationToken);

        public async Task<IReadOnlyList<MessageReceipt>> GetByMessageAsync(long organizationId, long conversationId, long messageId, CancellationToken cancellationToken = default) =>
            await _context.MessageReceipts
                .Where(r => r.OrganizationId == organizationId && r.ConversationId == conversationId && r.MessageId == messageId)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(MessageReceipt receipt, CancellationToken cancellationToken = default)
        {
            await _context.MessageReceipts.AddAsync(receipt, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<MessageReceipt> receipts, CancellationToken cancellationToken = default)
        {
            await _context.MessageReceipts.AddRangeAsync(receipts, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(MessageReceipt receipt, CancellationToken cancellationToken = default)
        {
            _context.MessageReceipts.Update(receipt);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
