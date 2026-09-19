using Microsoft.EntityFrameworkCore;
using Philo.Domain.Entities;
using Philo.Domain.Interfaces;

namespace Philo.Infrastructure.Persistence.Repositories
{
    public sealed class MessageRepository : BaseRepository<Message>, IMessageRepository
    {
        public MessageRepository(PhiloDbContext context) : base(context) { }

        public async Task<Message?> GetByIdAsync(long organizationId, long conversationId, long messageId, CancellationToken cancellationToken = default) =>
            await Set.FirstOrDefaultAsync(
                m => m.OrganizationId == organizationId && m.ConversationId == conversationId && m.Id == messageId, cancellationToken);

        public async Task<Message?> GetByClientMessageIdAsync(
            long organizationId, long conversationId, long senderId, Guid clientMessageId, CancellationToken cancellationToken = default) =>
            await Set.FirstOrDefaultAsync(
                m => m.OrganizationId == organizationId && m.ConversationId == conversationId
                     && m.SenderId == senderId && m.ClientMessageId == clientMessageId,
                cancellationToken);

        public async Task<IReadOnlyList<Message>> GetHistoryAsync(
            long organizationId, long conversationId, long requestingUserId,
            DateTime? cursorSentAt, long? cursorId, int limit, CancellationToken cancellationToken = default)
        {
            var utcNow = DateTime.UtcNow;

            var query =
                from m in Context.Messages
                where m.OrganizationId == organizationId && m.ConversationId == conversationId
                where m.ExpiresAt == null || m.ExpiresAt > utcNow
                where !Context.MessageUserPreferences.Any(pref =>
                    pref.OrganizationId == organizationId && pref.ConversationId == conversationId &&
                    pref.MessageId == m.Id && pref.UserId == requestingUserId && pref.HiddenAt != null)
                select m;

            if (cursorSentAt is not null)
                query = query.Where(m => m.SentAt < cursorSentAt || (m.SentAt == cursorSentAt && m.Id < cursorId));

            return await query
                .OrderByDescending(m => m.SentAt).ThenByDescending(m => m.Id)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetUnreadCountAsync(long organizationId, long conversationId, long userId, CancellationToken cancellationToken = default)
        {
            var utcNow = DateTime.UtcNow;

            var query =
                from r in Context.MessageReceipts
                join m in Context.Messages on
                    new { r.OrganizationId, r.ConversationId, r.MessageId } equals
                    new { m.OrganizationId, m.ConversationId, MessageId = m.Id }
                where r.OrganizationId == organizationId && r.ConversationId == conversationId && r.UserId == userId
                where r.ReadAt == null && m.DeletedAt == null
                where m.ExpiresAt == null || m.ExpiresAt > utcNow
                where !Context.MessageUserPreferences.Any(pref =>
                    pref.OrganizationId == organizationId && pref.ConversationId == conversationId &&
                    pref.MessageId == m.Id && pref.UserId == userId && pref.HiddenAt != null)
                select r;

            return await query.CountAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Message>> GetPendingWithoutRecipientsAsync(
            long organizationId, long conversationId, long excludingSenderId, CancellationToken cancellationToken = default)
        {
            var utcNow = DateTime.UtcNow;

            var query =
                from m in Context.Messages
                where m.OrganizationId == organizationId && m.ConversationId == conversationId
                where m.SenderId != excludingSenderId && m.DeletedAt == null
                where m.ExpiresAt == null || m.ExpiresAt > utcNow
                where !Context.MessageReceipts.Any(r =>
                    r.OrganizationId == organizationId && r.ConversationId == conversationId && r.MessageId == m.Id)
                select m;

            return await query.ToListAsync(cancellationToken);
        }
    }
}
