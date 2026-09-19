using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Philo.Application.Common.Interfaces;

namespace Philo.WebApi.Common
{
    [ApiController]
    [Authorize]
    public abstract class PhiloControllerBase : ControllerBase
    {
        protected readonly ISender Mediator;
        protected readonly ICurrentUserService CurrentUser;

        protected PhiloControllerBase(ISender mediator, ICurrentUserService currentUser)
        {
            Mediator = mediator;
            CurrentUser = currentUser;
        }

        /// <summary>
        /// A organização do payload/rota precisa bater com a do token autenticado — nunca se confia
        /// na empresa enviada pelo cliente como prova de autorização (README ponto 1).
        /// </summary>
        protected IActionResult? EnsureOrganization(long organizationId) =>
            CurrentUser.OrganizationId == organizationId ? null : Forbid();

        protected bool IsStaff => CurrentUser.Role is "Agent" or "Admin";
    }
}
