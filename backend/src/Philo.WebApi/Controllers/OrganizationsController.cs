using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Philo.Application.Common.Interfaces;
using Philo.Application.Organizations.CreateOrganization;
using Philo.WebApi.Auth;
using Philo.WebApi.Common;

namespace Philo.WebApi.Controllers
{
    /// <summary>Provisionamento administrativo. Para um único site/loja, cadastre somente uma organização (README).</summary>
    [Route("api/organizations")]
    [AllowAnonymous]
    [ServiceFilter(typeof(InternalApiKeyFilter))]
    public sealed class OrganizationsController : PhiloControllerBase
    {
        public OrganizationsController(ISender mediator, ICurrentUserService currentUser) : base(mediator, currentUser) { }

        public sealed record CreateOrganizationRequest(string Name);

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrganizationRequest request, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new CreateOrganizationCommand(request.Name), cancellationToken);
            return result.ToActionResult(this, id => CreatedAtAction(nameof(Create), new { id }, new { id }));
        }
    }
}
