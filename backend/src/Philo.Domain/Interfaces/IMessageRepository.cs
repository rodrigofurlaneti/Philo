using Philo.Domain.Entities;

namespace Philo.Domain.Interfaces
{
    public interface IMessageRepository : IBaseRepository<Message>
    {
        Task<Message?> GetByIdAsync(long organizationId, long conversationId, long messageId, CancellationToken cancellationToken = default);
        Task<Message?> GetByClientMessageIdAsync(long organizationId, long conversationId, long senderId, Guid clientMessageId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Message>> GetHistoryAsync(
            long organizationId, long conversationId, long requestingUserId,
            DateTime? cursorSentAt, long? cursorId, int limit, CancellationToken cancellationToken = default);
        Task<int> GetUnreadCountAsync(long organizationId, long conversationId, long userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Message>> GetPendingWithoutRecipientsAsync(long organizationId, long conversationId, long excludingSenderId, CancellationToken cancellationToken = default);
    }
}
