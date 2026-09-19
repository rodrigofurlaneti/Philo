using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philo.Domain.Entities;

namespace Philo.Infrastructure.Persistence.Configurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(u => u.DisplayName).HasColumnName("display_name").HasMaxLength(100).IsRequired();
            builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(254);
            builder.Property(u => u.PhoneE164).HasColumnName("phone_e164").HasMaxLength(16);
            builder.Property(u => u.AvatarObjectKey).HasColumnName("avatar_object_key").HasMaxLength(512);
            builder.Property(u => u.Status).HasColumnName("status").HasConversion(EnumConverters.UserStatusConverter).HasMaxLength(20).IsRequired();
            builder.Property(u => u.CreatedAt).HasColumnName("created_at").HasPrecision(6);
            builder.Property(u => u.UpdatedAt).HasColumnName("updated_at").HasPrecision(6);

            builder.HasMany(u => u.Memberships).WithOne(m => m.User).HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Metadata.FindNavigation(nameof(User.Memberships))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
