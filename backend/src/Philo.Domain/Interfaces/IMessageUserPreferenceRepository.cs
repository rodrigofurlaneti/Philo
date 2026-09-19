using Philo.Domain.Entities;

namespace Philo.Domain.Interfaces
{
    public interface IMessageUserPreferenceRepository
    {
        Task<MessageUserPreference?> GetAsync(long organizationId, long conversationId, long messageId, long userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<MessageUserPreference>> GetStarredAsync(long organizationId, long userId, int limit, CancellationToken cancellationToken = default);
        Task AddAsync(MessageUserPreference preference, CancellationToken cancellationToken = default);
        Task UpdateAsync(MessageUserPreference preference, CancellationToken cancellationToken = default);
    }
}
