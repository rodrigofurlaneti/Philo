using FluentValidation;
using MediatR;
using Philo.Domain.Entities;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Products.CreateProduct
{
    public sealed record CreateProductCommand(long OrganizationId, string Name, string? Sku, string? PageUrl) : IRequest<Result<long>>;

    public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Sku).MaximumLength(64);
            RuleFor(x => x.PageUrl).MaximumLength(2048);
        }
    }

    public sealed class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<long>>
    {
        private readonly IProductRepository _products;

        public CreateProductHandler(IProductRepository products) => _products = products;

        public async Task<Result<long>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.Sku) &&
                await _products.SkuExistsAsync(request.OrganizationId, request.Sku, cancellationToken))
                return Result.Failure<long>(DomainErrors.Product.DuplicateSku);

            var result = Product.Create(request.OrganizationId, request.Name, request.Sku, request.PageUrl);
            if (result.IsFailure)
                return Result.Failure<long>(result.Error);

            await _products.AddAsync(result.Value, cancellationToken);
            return Result.Success(result.Value.Id);
        }
    }
}
