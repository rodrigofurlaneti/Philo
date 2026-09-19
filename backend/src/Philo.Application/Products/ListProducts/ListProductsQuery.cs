using MediatR;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Products.ListProducts
{
    public sealed record ProductDto(long Id, string? Sku, string Name, string? PageUrl, string Status);

    public sealed record ListProductsQuery(long OrganizationId) : IRequest<Result<IReadOnlyList<ProductDto>>>;

    public sealed class ListProductsHandler : IRequestHandler<ListProductsQuery, Result<IReadOnlyList<ProductDto>>>
    {
        private readonly IProductRepository _products;

        public ListProductsHandler(IProductRepository products) => _products = products;

        public async Task<Result<IReadOnlyList<ProductDto>>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _products.ListAsync(request.OrganizationId, cancellationToken);
            IReadOnlyList<ProductDto> dtos = products
                .Select(p => new ProductDto(p.Id, p.Sku, p.Name, p.PageUrl, p.Status.ToString()))
                .ToList();

            return Result.Success(dtos);
        }
    }
}
