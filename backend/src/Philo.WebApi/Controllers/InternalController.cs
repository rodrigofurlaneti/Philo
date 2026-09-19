using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Philo.Application.Auth.IssueStaffSession;
using Philo.Application.Common.Interfaces;
using Philo.WebApi.Auth;
using Philo.WebApi.Common;

namespace Philo.WebApi.Controllers
{
    /// <summary>
    /// Endpoints chamados pelo backend do site (não pelo navegador). O site já autenticou o
    /// usuário por seus próprios meios; aqui só emitimos a sessão JWT do Philo para ele.
    /// </summary>
    [Route("api/internal")]
    [AllowAnonymous]
    [ServiceFilter(typeof(InternalApiKeyFilter))]
    public sealed class InternalController : PhiloControllerBase
    {
        public InternalController(ISender mediator, ICurrentUserService currentUser) : base(mediator, currentUser) { }

        public sealed record IssueSessionRequest(long OrganizationId, long UserId);

        [HttpPost("sessions")]
        public async Task<IActionResult> IssueSession(IssueSessionRequest request, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new IssueStaffSessionCommand(request.OrganizationId, request.UserId), cancellationToken);
            return result.ToActionResult(this);
        }
    }
}
