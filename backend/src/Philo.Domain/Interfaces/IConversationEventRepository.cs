using Philo.Domain.Entities;

namespace Philo.Domain.Interfaces
{
    public interface IConversationEventRepository
    {
        Task AddAsync(ConversationEvent conversationEvent, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ConversationEvent>> GetHistoryAsync(long organizationId, long conversationId, int limit, CancellationToken cancellationToken = default);
    }
}
