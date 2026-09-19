using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Philo.Application.Auth.Common;
using Philo.Application.Common.Interfaces;
using Philo.Domain.Entities;
using Philo.Domain.Enums;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Auth.CreateVisitorSession
{
    /// <summary>
    /// Emite uma sessão opaca e protegida para um visitante não cadastrado (cadastro por telefone
    /// não é obrigatório). Cria o usuário global e o vínculo customer na organização.
    /// </summary>
    public sealed record CreateVisitorSessionCommand(long OrganizationId, string? DisplayName) : IRequest<Result<SessionResponse>>;

    public sealed class CreateVisitorSessionValidator : AbstractValidator<CreateVisitorSessionCommand>
    {
        public CreateVisitorSessionValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.DisplayName).MaximumLength(100);
        }
    }

    public sealed class CreateVisitorSessionHandler : IRequestHandler<CreateVisitorSessionCommand, Result<SessionResponse>>
    {
        private readonly IOrganizationRepository _organizations;
        private readonly IUserRepository _users;
        private readonly IOrganizationUserRepository _memberships;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenService _tokenService;
        private readonly ILogger<CreateVisitorSessionHandler> _logger;

        public CreateVisitorSessionHandler(
            IOrganizationRepository organizations, IUserRepository users, IOrganizationUserRepository memberships,
            IUnitOfWork unitOfWork, IJwtTokenService tokenService, ILogger<CreateVisitorSessionHandler> logger)
        {
            _organizations = organizations;
            _users = users;
            _memberships = memberships;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<Result<SessionResponse>> Handle(CreateVisitorSessionCommand request, CancellationToken cancellationToken)
        {
            var organization = await _organizations.GetByIdAsync(request.OrganizationId, cancellationToken);
            if (organization is null)
                return Result.Failure<SessionResponse>(DomainErrors.Organization.NotFound(request.OrganizationId));
            if (!organization.IsActive)
                return Result.Failure<SessionResponse>(DomainErrors.Organization.Suspended);

            return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var userResult = User.Create(string.IsNullOrWhiteSpace(request.DisplayName) ? "Visitante" : request.DisplayName!);
                if (userResult.IsFailure)
                    return Result.Failure<SessionResponse>(userResult.Error);

                var user = userResult.Value;
                await _users.AddAsync(user, ct);

                var membershipResult = OrganizationUser.Create(organization.Id, user.Id, Role.Customer);
                if (membershipResult.IsFailure)
                    return Result.Failure<SessionResponse>(membershipResult.Error);

                await _memberships.AddAsync(membershipResult.Value, ct);

                var sessionId = Guid.NewGuid().ToString("N");
                var token = _tokenService.IssueToken(user.Id, organization.Id, Role.Customer.ToString(), sessionId);

                _logger.LogInformation("Sessão de visitante criada: usuário {UserId} na organização {OrganizationId}", user.Id, organization.Id);

                return Result.Success(new SessionResponse(token.AccessToken, token.ExpiresAtUtc, user.Id, organization.Id, Role.Customer.ToString()));
            }, cancellationToken);
        }
    }
}
