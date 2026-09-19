using Microsoft.EntityFrameworkCore;
using Philo.Domain.Entities;
using Philo.Domain.Interfaces;

namespace Philo.Infrastructure.Persistence.Repositories
{
    public sealed class MessageAttachmentRepository : IMessageAttachmentRepository
    {
        private readonly PhiloDbContext _context;

        public MessageAttachmentRepository(PhiloDbContext context) => _context = context;

        public async Task<MessageAttachment?> GetAsync(long organizationId, long conversationId, long attachmentId, CancellationToken cancellationToken = default) =>
            await _context.MessageAttachments.FirstOrDefaultAsync(
                a => a.OrganizationId == organizationId && a.ConversationId == conversationId && a.Id == attachmentId, cancellationToken);

        public async Task<IReadOnlyList<MessageAttachment>> GetByMessageAsync(long organizationId, long conversationId, long messageId, CancellationToken cancellationToken = default) =>
            await _context.MessageAttachments
                .Where(a => a.OrganizationId == organizationId && a.ConversationId == conversationId && a.MessageId == messageId)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(MessageAttachment attachment, CancellationToken cancellationToken = default)
        {
            await _context.MessageAttachments.AddAsync(attachment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
