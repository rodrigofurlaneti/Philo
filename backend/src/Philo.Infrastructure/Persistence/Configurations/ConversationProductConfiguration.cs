using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philo.Domain.Entities;

namespace Philo.Infrastructure.Persistence.Configurations
{
    public sealed class ConversationProductConfiguration : IEntityTypeConfiguration<ConversationProduct>
    {
        public void Configure(EntityTypeBuilder<ConversationProduct> builder)
        {
            builder.ToTable("conversation_products");
            builder.HasKey(cp => new { cp.OrganizationId, cp.ConversationId, cp.ProductId });
            builder.Property(cp => cp.OrganizationId).HasColumnName("organization_id");
            builder.Property(cp => cp.ConversationId).HasColumnName("conversation_id");
            builder.Property(cp => cp.ProductId).HasColumnName("product_id");
            builder.Property(cp => cp.ProductNameSnapshot).HasColumnName("product_name_snapshot").HasMaxLength(200).IsRequired();
            builder.Property(cp => cp.CreatedAt).HasColumnName("created_at").HasPrecision(6);

            builder.HasOne(cp => cp.Product).WithMany()
                .HasForeignKey(cp => new { cp.OrganizationId, cp.ProductId })
                .HasPrincipalKey(p => new { p.OrganizationId, p.Id })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
