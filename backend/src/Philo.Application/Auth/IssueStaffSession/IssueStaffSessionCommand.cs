using FluentValidation;
using MediatR;
using Philo.Application.Auth.Common;
using Philo.Application.Common.Interfaces;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Auth.IssueStaffSession
{
    /// <summary>
    /// Endpoint interno: o site já autenticou o usuário por seus próprios meios e pede ao Philo
    /// para emitir uma sessão JWT para ele. Protegido por credencial de serviço na WebApi, nunca
    /// aceita papel/empresa vindos do chamador — o papel é sempre lido de organization_users.
    /// </summary>
    public sealed record IssueStaffSessionCommand(long OrganizationId, long UserId) : IRequest<Result<SessionResponse>>;

    public sealed class IssueStaffSessionValidator : AbstractValidator<IssueStaffSessionCommand>
    {
        public IssueStaffSessionValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.UserId).GreaterThan(0);
        }
    }

    public sealed class IssueStaffSessionHandler : IRequestHandler<IssueStaffSessionCommand, Result<SessionResponse>>
    {
        private readonly IOrganizationRepository _organizations;
        private readonly IUserRepository _users;
        private readonly IOrganizationUserRepository _memberships;
        private readonly IJwtTokenService _tokenService;

        public IssueStaffSessionHandler(
            IOrganizationRepository organizations, IUserRepository users,
            IOrganizationUserRepository memberships, IJwtTokenService tokenService)
        {
            _organizations = organizations;
            _users = users;
            _memberships = memberships;
            _tokenService = tokenService;
        }

        public async Task<Result<SessionResponse>> Handle(IssueStaffSessionCommand request, CancellationToken cancellationToken)
        {
            var organization = await _organizations.GetByIdAsync(request.OrganizationId, cancellationToken);
            if (organization is null)
                return Result.Failure<SessionResponse>(DomainErrors.Organization.NotFound(request.OrganizationId));
            if (!organization.IsActive)
                return Result.Failure<SessionResponse>(DomainErrors.Organization.Suspended);

            var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null || !user.IsActive)
                return Result.Failure<SessionResponse>(DomainErrors.User.NotFound);

            var membership = await _memberships.GetAsync(request.OrganizationId, request.UserId, cancellationToken);
            if (membership is null)
                return Result.Failure<SessionResponse>(DomainErrors.Membership.NotFound);
            if (!membership.IsActive)
                return Result.Failure<SessionResponse>(DomainErrors.Membership.Suspended);

            var sessionId = Guid.NewGuid().ToString("N");
            var token = _tokenService.IssueToken(user.Id, organization.Id, membership.Role.ToString(), sessionId);

            return Result.Success(new SessionResponse(token.AccessToken, token.ExpiresAtUtc, user.Id, organization.Id, membership.Role.ToString()));
        }
    }
}
