using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Philo.Application.Common.Interfaces;

namespace Philo.Infrastructure.Services
{
    /// <summary>Lê a identidade exclusivamente do ClaimsPrincipal autenticado (nunca do corpo da requisição).</summary>
    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly ClaimsPrincipal? _user;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor) =>
            _user = httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => _user?.Identity?.IsAuthenticated == true;

        public long UserId => long.TryParse(_user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? _user?.FindFirstValue("sub"), out var id) ? id : 0;

        public long OrganizationId => long.TryParse(_user?.FindFirstValue("org"), out var id) ? id : 0;

        public string Role => _user?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        public string SessionId => _user?.FindFirstValue("jti") ?? string.Empty;
    }
}
