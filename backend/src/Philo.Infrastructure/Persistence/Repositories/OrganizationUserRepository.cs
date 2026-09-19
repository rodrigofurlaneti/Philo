using Microsoft.EntityFrameworkCore;
using Philo.Domain.Entities;
using Philo.Domain.Interfaces;

namespace Philo.Infrastructure.Persistence.Repositories
{
    public sealed class OrganizationUserRepository : IOrganizationUserRepository
    {
        private readonly PhiloDbContext _context;

        public OrganizationUserRepository(PhiloDbContext context) => _context = context;

        public async Task<OrganizationUser?> GetAsync(long organizationId, long userId, CancellationToken cancellationToken = default) =>
            await _context.OrganizationUsers.FirstOrDefaultAsync(
                m => m.OrganizationId == organizationId && m.UserId == userId, cancellationToken);

        public async Task<IReadOnlyList<OrganizationUser>> GetTeamAsync(long organizationId, CancellationToken cancellationToken = default) =>
            await _context.OrganizationUsers
                .Where(m => m.OrganizationId == organizationId)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(OrganizationUser membership, CancellationToken cancellationToken = default)
        {
            await _context.OrganizationUsers.AddAsync(membership, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(OrganizationUser membership, CancellationToken cancellationToken = default)
        {
            _context.OrganizationUsers.Update(membership);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
