using Philo.Domain.Entities;

namespace Philo.Domain.Interfaces
{
    public interface IConversationProductRepository
    {
        Task<IReadOnlyList<ConversationProduct>> GetAsync(long organizationId, long conversationId, CancellationToken cancellationToken = default);
        Task AddAsync(ConversationProduct link, CancellationToken cancellationToken = default);
    }
}
