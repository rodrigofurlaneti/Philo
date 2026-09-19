using Microsoft.EntityFrameworkCore;
using Philo.Domain.Entities;
using Philo.Domain.Interfaces;

namespace Philo.Infrastructure.Persistence.Repositories
{
    public sealed class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(PhiloDbContext context) : base(context) { }

        public async Task<Product?> GetByIdForOrganizationAsync(long organizationId, long productId, CancellationToken cancellationToken = default) =>
            await Set.FirstOrDefaultAsync(p => p.OrganizationId == organizationId && p.Id == productId, cancellationToken);

        public async Task<bool> SkuExistsAsync(long organizationId, string sku, CancellationToken cancellationToken = default) =>
            await Set.AnyAsync(p => p.OrganizationId == organizationId && p.Sku == sku, cancellationToken);

        public async Task<IReadOnlyList<Product>> ListAsync(long organizationId, CancellationToken cancellationToken = default) =>
            await Set.Where(p => p.OrganizationId == organizationId).OrderBy(p => p.Name).ToListAsync(cancellationToken);
    }
}
