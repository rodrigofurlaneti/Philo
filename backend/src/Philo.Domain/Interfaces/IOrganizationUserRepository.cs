using Philo.Domain.Entities;

namespace Philo.Domain.Interfaces
{
    public interface IOrganizationUserRepository
    {
        Task<OrganizationUser?> GetAsync(long organizationId, long userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<OrganizationUser>> GetTeamAsync(long organizationId, CancellationToken cancellationToken = default);
        Task AddAsync(OrganizationUser membership, CancellationToken cancellationToken = default);
        Task UpdateAsync(OrganizationUser membership, CancellationToken cancellationToken = default);
    }
}
