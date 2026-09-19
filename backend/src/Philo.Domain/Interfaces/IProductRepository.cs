using Philo.Domain.Entities;

namespace Philo.Domain.Interfaces
{
    public interface IProductRepository : IBaseRepository<Product>
    {
        Task<Product?> GetByIdForOrganizationAsync(long organizationId, long productId, CancellationToken cancellationToken = default);
        Task<bool> SkuExistsAsync(long organizationId, string sku, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Product>> ListAsync(long organizationId, CancellationToken cancellationToken = default);
    }
}
