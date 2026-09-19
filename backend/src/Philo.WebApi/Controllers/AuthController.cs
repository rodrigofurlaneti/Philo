using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Philo.Application.Auth.CreateVisitorSession;
using Philo.Application.Common.Interfaces;
using Philo.WebApi.Common;

namespace Philo.WebApi.Controllers
{
    [Route("api/auth")]
    public sealed class AuthController : PhiloControllerBase
    {
        public AuthController(ISender mediator, ICurrentUserService currentUser) : base(mediator, currentUser) { }

        public sealed record CreateVisitorSessionRequest(long OrganizationId, string? DisplayName);

        /// <summary>Emite uma sessão para um visitante não cadastrado (cadastro por telefone não é obrigatório).</summary>
        [AllowAnonymous]
        [HttpPost("visitor-sessions")]
        public async Task<IActionResult> CreateVisitorSession(CreateVisitorSessionRequest request, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new CreateVisitorSessionCommand(request.OrganizationId, request.DisplayName), cancellationToken);
            return result.ToActionResult(this);
        }
    }
}
