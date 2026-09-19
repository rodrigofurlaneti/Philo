using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philo.Domain.Entities;

namespace Philo.Infrastructure.Persistence.Configurations
{
    public sealed class OrganizationUserConfiguration : IEntityTypeConfiguration<OrganizationUser>
    {
        public void Configure(EntityTypeBuilder<OrganizationUser> builder)
        {
            builder.ToTable("organization_users");
            builder.HasKey(ou => new { ou.OrganizationId, ou.UserId });
            builder.Property(ou => ou.OrganizationId).HasColumnName("organization_id");
            builder.Property(ou => ou.UserId).HasColumnName("user_id");
            builder.Property(ou => ou.Role).HasColumnName("role").HasConversion(EnumConverters.RoleConverter).HasMaxLength(20).IsRequired();
            builder.Property(ou => ou.Status).HasColumnName("status").HasConversion(EnumConverters.MembershipStatusConverter).HasMaxLength(20).IsRequired();
            builder.Property(ou => ou.CreatedAt).HasColumnName("created_at").HasPrecision(6);

            builder.HasIndex(ou => new { ou.UserId, ou.OrganizationId }).HasDatabaseName("idx_ou_user");
            builder.HasIndex(ou => new { ou.OrganizationId, ou.Role, ou.Status }).HasDatabaseName("idx_ou_team");
        }
    }
}
