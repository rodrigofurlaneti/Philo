using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philo.Domain.Entities;

namespace Philo.Infrastructure.Persistence.Configurations
{
    public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(p => p.OrganizationId).HasColumnName("organization_id");
            builder.Property(p => p.Sku).HasColumnName("sku").HasMaxLength(64);
            builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(p => p.PageUrl).HasColumnName("page_url").HasMaxLength(2048);
            builder.Property(p => p.Status).HasColumnName("status").HasConversion(EnumConverters.ProductStatusConverter).HasMaxLength(20).IsRequired();
            builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasPrecision(6);
            builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").HasPrecision(6);

            builder.HasAlternateKey(p => new { p.OrganizationId, p.Id }).HasName("uq_product_org");
            builder.HasIndex(p => new { p.OrganizationId, p.Sku }).IsUnique().HasDatabaseName("uq_product_sku");

            builder.HasOne(p => p.Organization).WithMany(o => o.Products).HasForeignKey(p => p.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
