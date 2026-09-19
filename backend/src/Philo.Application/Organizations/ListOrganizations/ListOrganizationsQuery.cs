using MediatR;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Organizations.ListOrganizations
{
    /// <summary>Uso interno/dev (ex.: painel de login rápido da equipe) — protegido por API key na WebApi.</summary>
    public sealed record OrganizationSummaryDto(long Id, string Name, string Status);

    public sealed record ListOrganizationsQuery : IRequest<Result<IReadOnlyList<OrganizationSummaryDto>>>;

    public sealed class ListOrganizationsHandler : IRequestHandler<ListOrganizationsQuery, Result<IReadOnlyList<OrganizationSummaryDto>>>
    {
        private readonly IOrganizationRepository _organizations;

        public ListOrganizationsHandler(IOrganizationRepository organizations) => _organizations = organizations;

        public async Task<Result<IReadOnlyList<OrganizationSummaryDto>>> Handle(ListOrganizationsQuery request, CancellationToken cancellationToken)
        {
            var organizations = await _organizations.GetAllAsync(cancellationToken);
            IReadOnlyList<OrganizationSummaryDto> dtos = organizations
                .Where(o => o.IsActive)
                .Select(o => new OrganizationSummaryDto(o.Id, o.Name, o.Status.ToString()))
                .ToList();

            return Result.Success(dtos);
        }
    }
}
