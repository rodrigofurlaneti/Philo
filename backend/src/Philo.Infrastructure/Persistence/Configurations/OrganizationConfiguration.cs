using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philo.Domain.Entities;

namespace Philo.Infrastructure.Persistence.Configurations
{
    public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.ToTable("organizations");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(o => o.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            builder.Property(o => o.Status).HasColumnName("status").HasConversion(EnumConverters.OrganizationStatusConverter).HasMaxLength(20).IsRequired();
            builder.Property(o => o.CreatedAt).HasColumnName("created_at").HasPrecision(6);

            builder.HasMany(o => o.Members).WithOne(m => m.Organization).HasForeignKey(m => m.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Metadata.FindNavigation(nameof(Organization.Members))!.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(o => o.Products).WithOne(p => p.Organization).HasForeignKey(p => p.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Metadata.FindNavigation(nameof(Organization.Products))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
