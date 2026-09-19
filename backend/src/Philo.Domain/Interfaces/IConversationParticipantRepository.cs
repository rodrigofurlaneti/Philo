using Philo.Domain.Entities;

namespace Philo.Domain.Interfaces
{
    public interface IConversationParticipantRepository
    {
        Task<ConversationParticipant?> GetAsync(long organizationId, long conversationId, long userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ConversationParticipant>> GetActiveAsync(long organizationId, long conversationId, CancellationToken cancellationToken = default);
        Task<bool> IsActiveParticipantAsync(long organizationId, long conversationId, long userId, CancellationToken cancellationToken = default);
        Task AddAsync(ConversationParticipant participant, CancellationToken cancellationToken = default);
        Task UpdateAsync(ConversationParticipant participant, CancellationToken cancellationToken = default);
    }
}
