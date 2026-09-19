using Philo.Domain.Entities;

namespace Philo.Domain.Interfaces
{
    public interface IConversationRepository : IBaseRepository<Conversation>
    {
        Task<Conversation?> GetByIdForOrganizationAsync(long organizationId, long conversationId, CancellationToken cancellationToken = default);
        Task<Conversation?> GetWithParticipantsAsync(long organizationId, long conversationId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Conversation>> GetQueueAsync(long organizationId, string? status, int limit, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Conversation>> GetForCustomerAsync(long organizationId, long customerId, int limit, CancellationToken cancellationToken = default);
    }
}
