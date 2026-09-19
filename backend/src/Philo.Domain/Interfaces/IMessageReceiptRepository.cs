using Philo.Domain.Entities;

namespace Philo.Domain.Interfaces
{
    public interface IMessageReceiptRepository
    {
        Task<MessageReceipt?> GetAsync(long organizationId, long conversationId, long messageId, long userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<MessageReceipt>> GetByMessageAsync(long organizationId, long conversationId, long messageId, CancellationToken cancellationToken = default);
        Task AddAsync(MessageReceipt receipt, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<MessageReceipt> receipts, CancellationToken cancellationToken = default);
        Task UpdateAsync(MessageReceipt receipt, CancellationToken cancellationToken = default);
    }
}
