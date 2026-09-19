using FluentValidation;
using MediatR;
using Philo.Domain.Entities;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Organizations.CreateOrganization
{
    /// <summary>Para um único site/loja, cadastre somente uma organização (README).</summary>
    public sealed record CreateOrganizationCommand(string Name) : IRequest<Result<long>>;

    public sealed class CreateOrganizationValidator : AbstractValidator<CreateOrganizationCommand>
    {
        public CreateOrganizationValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        }
    }

    public sealed class CreateOrganizationHandler : IRequestHandler<CreateOrganizationCommand, Result<long>>
    {
        private readonly IOrganizationRepository _organizations;

        public CreateOrganizationHandler(IOrganizationRepository organizations) => _organizations = organizations;

        public async Task<Result<long>> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
        {
            var result = Organization.Create(request.Name);
            if (result.IsFailure)
                return Result.Failure<long>(result.Error);

            await _organizations.AddAsync(result.Value, cancellationToken);
            return Result.Success(result.Value.Id);
        }
    }
}
