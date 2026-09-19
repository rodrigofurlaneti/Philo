using FluentValidation;
using MediatR;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Products.UpdateProduct
{
    public sealed record UpdateProductCommand(long OrganizationId, long ProductId, string Name, string? Sku, string? PageUrl) : IRequest<Result>;

    public sealed class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ProductId).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        }
    }

    public sealed class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Result>
    {
        private readonly IProductRepository _products;

        public UpdateProductHandler(IProductRepository products) => _products = products;

        public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _products.GetByIdForOrganizationAsync(request.OrganizationId, request.ProductId, cancellationToken);
            if (product is null)
                return Result.Failure(DomainErrors.Product.NotFound);

            if (!string.IsNullOrWhiteSpace(request.Sku) &&
                !string.Equals(request.Sku, product.Sku, StringComparison.Ordinal) &&
                await _products.SkuExistsAsync(request.OrganizationId, request.Sku, cancellationToken))
                return Result.Failure(DomainErrors.Product.DuplicateSku);

            product.Update(request.Name, request.Sku, request.PageUrl);
            await _products.UpdateAsync(product, cancellationToken);
            return Result.Success();
        }
    }
}
