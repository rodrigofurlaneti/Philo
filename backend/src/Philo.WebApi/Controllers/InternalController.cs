using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Philo.Application.Auth.IssueStaffSession;
using Philo.Application.Auth.ListStaffMembers;
using Philo.Application.Common.Interfaces;
using Philo.Application.Organizations.ListOrganizations;
using Philo.WebApi.Auth;
using Philo.WebApi.Common;

namespace Philo.WebApi.Controllers
{
    /// <summary>
    /// Endpoints chamados pelo backend do site (não pelo navegador) — ou, em desenvolvimento, pelo
    /// painel de login rápido da equipe do próprio frontend Philo. O site já autenticou o usuário
    /// por seus próprios meios; aqui só emitimos a sessão JWT do Philo para ele.
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

        /// <summary>Uso interno/dev (painel de login rápido da equipe): lista as organizações ativas.</summary>
        [HttpGet("organizations")]
        public async Task<IActionResult> ListOrganizations(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new ListOrganizationsQuery(), cancellationToken);
            return result.ToActionResult(this);
        }

        /// <summary>Uso interno/dev (painel de login rápido da equipe): lista os membros ativos de uma organização.</summary>
        [HttpGet("organizations/{organizationId:long}/members")]
        public async Task<IActionResult> ListMembers(long organizationId, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new ListStaffMembersQuery(organizationId), cancellationToken);
            return result.ToActionResult(this);
        }
    }
}
