using MediatR;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Auth.ListStaffMembers
{
    /// <summary>Uso interno/dev (ex.: painel de login rápido da equipe) — protegido por API key na WebApi.</summary>
    public sealed record StaffMemberDto(long UserId, string DisplayName, string? Email, string Role);

    public sealed record ListStaffMembersQuery(long OrganizationId) : IRequest<Result<IReadOnlyList<StaffMemberDto>>>;

    public sealed class ListStaffMembersHandler : IRequestHandler<ListStaffMembersQuery, Result<IReadOnlyList<StaffMemberDto>>>
    {
        private readonly IOrganizationUserRepository _memberships;

        public ListStaffMembersHandler(IOrganizationUserRepository memberships) => _memberships = memberships;

        public async Task<Result<IReadOnlyList<StaffMemberDto>>> Handle(ListStaffMembersQuery request, CancellationToken cancellationToken)
        {
            var team = await _memberships.GetTeamAsync(request.OrganizationId, cancellationToken);
            IReadOnlyList<StaffMemberDto> dtos = team
                .Where(m => m.IsActive && m.User is { } user && user.IsActive)
                .OrderBy(m => m.User!.DisplayName)
                .Select(m => new StaffMemberDto(m.UserId, m.User!.DisplayName, m.User!.Email, m.Role.ToString()))
                .ToList();

            return Result.Success(dtos);
        }
    }
}
