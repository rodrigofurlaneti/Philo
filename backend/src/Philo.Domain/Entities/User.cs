using Philo.Domain.Enums;
using Philo.Domain.Primitives;

namespace Philo.Domain.Entities
{
    public sealed class User : AggregateRoot
    {
        public string DisplayName { get; private set; } = null!;
        public string? Email { get; private set; }
        public string? PhoneE164 { get; private set; }
        public string? AvatarObjectKey { get; private set; }
        public UserStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private readonly List<OrganizationUser> _memberships = [];
        public IReadOnlyCollection<OrganizationUser> Memberships => _memberships.AsReadOnly();

        private User() : base(0) { }

        private User(string displayName, string? email, string? phoneE164) : base(0)
        {
            DisplayName = displayName;
            Email = email;
            PhoneE164 = phoneE164;
            Status = UserStatus.Active;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public static Result<User> Create(string displayName, string? email = null, string? phoneE164 = null)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return Result.Failure<User>(Error.Validation("User.DisplayNameRequired", "O nome de exibição é obrigatório."));

            return Result.Success(new User(displayName.Trim(), email, phoneE164));
        }

        /// <summary>Cria um usuário-visitante anônimo, identificado apenas por sessão.</summary>
        public static User CreateVisitor(string displayName = "Visitante") => new(displayName, null, null);

        public void UpdateProfile(string displayName, string? email, string? phoneE164)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("O nome de exibição é obrigatório.", nameof(displayName));

            DisplayName = displayName.Trim();
            Email = email;
            PhoneE164 = phoneE164;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAvatar(string? objectKey)
        {
            AvatarObjectKey = objectKey;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Suspend()
        {
            Status = UserStatus.Suspended;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reactivate()
        {
            Status = UserStatus.Active;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Anonymize()
        {
            DisplayName = "Usuário removido";
            Email = null;
            PhoneE164 = null;
            AvatarObjectKey = null;
            Status = UserStatus.Anonymized;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsActive => Status == UserStatus.Active;
    }
}
