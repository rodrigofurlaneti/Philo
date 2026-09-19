using MediatR;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Products.DeactivateProduct
{
    public sealed record DeactivateProductCommand(long OrganizationId, long ProductId) : IRequest<Result>;

    public sealed class DeactivateProductHandler : IRequestHandler<DeactivateProductCommand, Result>
    {
        private readonly IProductRepository _products;

        public DeactivateProductHandler(IProductRepository products) => _products = products;

        public async Task<Result> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _products.GetByIdForOrganizationAsync(request.OrganizationId, request.ProductId, cancellationToken);
            if (product is null)
                return Result.Failure(DomainErrors.Product.NotFound);

            product.Deactivate();
            await _products.UpdateAsync(product, cancellationToken);
            return Result.Success();
        }
    }
}
