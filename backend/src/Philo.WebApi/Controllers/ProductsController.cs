using MediatR;
using Microsoft.AspNetCore.Mvc;
using Philo.Application.Common.Interfaces;
using Philo.Application.Products.CreateProduct;
using Philo.Application.Products.DeactivateProduct;
using Philo.Application.Products.ListProducts;
using Philo.Application.Products.UpdateProduct;
using Philo.WebApi.Common;

namespace Philo.WebApi.Controllers
{
    [Route("api/organizations/{organizationId:long}/products")]
    public sealed class ProductsController : PhiloControllerBase
    {
        public ProductsController(ISender mediator, ICurrentUserService currentUser) : base(mediator, currentUser) { }

        public sealed record CreateProductRequest(string Name, string? Sku, string? PageUrl);
        public sealed record UpdateProductRequest(string Name, string? Sku, string? PageUrl);

        [HttpGet]
        public async Task<IActionResult> List(long organizationId, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;

            var result = await Mediator.Send(new ListProductsQuery(organizationId), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPost]
        public async Task<IActionResult> Create(long organizationId, CreateProductRequest request, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;
            if (!IsStaff) return Forbid();

            var result = await Mediator.Send(new CreateProductCommand(organizationId, request.Name, request.Sku, request.PageUrl), cancellationToken);
            return result.ToActionResult(this, id => CreatedAtAction(nameof(List), new { organizationId }, new { id }));
        }

        [HttpPut("{productId:long}")]
        public async Task<IActionResult> Update(long organizationId, long productId, UpdateProductRequest request, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;
            if (!IsStaff) return Forbid();

            var result = await Mediator.Send(new UpdateProductCommand(organizationId, productId, request.Name, request.Sku, request.PageUrl), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpDelete("{productId:long}")]
        public async Task<IActionResult> Deactivate(long organizationId, long productId, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;
            if (!IsStaff) return Forbid();

            var result = await Mediator.Send(new DeactivateProductCommand(organizationId, productId), cancellationToken);
            return result.ToActionResult(this);
        }
    }
}
