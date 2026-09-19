using Philo.Domain.Enums;
using Philo.Domain.Primitives;

namespace Philo.Domain.Entities
{
    /// <summary>Vínculo (organization_id, user_id): papel e status do usuário dentro de uma organização.</summary>
    public sealed class OrganizationUser
    {
        public long OrganizationId { get; private set; }
        public long UserId { get; private set; }
        public Role Role { get; private set; }
        public MembershipStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Organization? Organization { get; private set; }
        public User? User { get; private set; }

        private OrganizationUser() { }

        private OrganizationUser(long organizationId, long userId, Role role)
        {
            OrganizationId = organizationId;
            UserId = userId;
            Role = role;
            Status = MembershipStatus.Active;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<OrganizationUser> Create(long organizationId, long userId, Role role)
        {
            if (organizationId <= 0 || userId <= 0)
                return Result.Failure<OrganizationUser>(Error.Validation("Membership.InvalidIds", "Organização e usuário são obrigatórios."));

            return Result.Success(new OrganizationUser(organizationId, userId, role));
        }

        public void Suspend() => Status = MembershipStatus.Suspended;
        public void Reactivate() => Status = MembershipStatus.Active;
        public bool IsActive => Status == MembershipStatus.Active;
    }
}
