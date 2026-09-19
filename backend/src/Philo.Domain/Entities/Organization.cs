using Philo.Domain.Enums;
using Philo.Domain.Primitives;

namespace Philo.Domain.Entities
{
    public sealed class Organization : AggregateRoot
    {
        public string Name { get; private set; } = null!;
        public OrganizationStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private readonly List<OrganizationUser> _members = [];
        public IReadOnlyCollection<OrganizationUser> Members => _members.AsReadOnly();

        private readonly List<Product> _products = [];
        public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

        private Organization() : base(0) { }

        private Organization(string name) : base(0)
        {
            Name = name;
            Status = OrganizationStatus.Active;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<Organization> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<Organization>(Error.Validation("Organization.NameRequired", "O nome da organização é obrigatório."));

            return Result.Success(new Organization(name.Trim()));
        }

        public void Suspend() => Status = OrganizationStatus.Suspended;
        public void Reactivate() => Status = OrganizationStatus.Active;
        public bool IsActive => Status == OrganizationStatus.Active;
    }
}
