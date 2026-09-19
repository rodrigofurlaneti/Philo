using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philo.Domain.Entities;

namespace Philo.Infrastructure.Persistence.Configurations
{
    public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            builder.ToTable("conversations", t => t.HasCheckConstraint(
                "ck_conversation_closed",
                "(status = 'closed' AND closed_at IS NOT NULL AND closed_at >= created_at) OR (status <> 'closed' AND closed_at IS NULL)"));

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(c => c.OrganizationId).HasColumnName("organization_id");
            builder.Property(c => c.CustomerId).HasColumnName("customer_id");
            builder.Property(c => c.AssignedTo).HasColumnName("assigned_to");
            builder.Property(c => c.CreatedBy).HasColumnName("created_by");
            builder.Property(c => c.Purpose).HasColumnName("purpose").HasConversion(EnumConverters.ConversationPurposeConverter).HasMaxLength(20).IsRequired();
            builder.Property(c => c.Subject).HasColumnName("subject").HasMaxLength(200);
            builder.Property(c => c.Status).HasColumnName("status").HasConversion(EnumConverters.ConversationStatusConverter).HasMaxLength(20).IsRequired();
            builder.Property(c => c.Priority).HasColumnName("priority").HasConversion(EnumConverters.PriorityConverter).HasMaxLength(20).IsRequired();
            builder.Property(c => c.SourcePageUrl).HasColumnName("source_page_url").HasMaxLength(2048);
            builder.Property(c => c.LastActivityAt).HasColumnName("last_activity_at").HasPrecision(6);
            builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasPrecision(6);
            builder.Property(c => c.UpdatedAt).HasColumnName("updated_at").HasPrecision(6);
            builder.Property(c => c.ClosedAt).HasColumnName("closed_at").HasPrecision(6);

            builder.HasAlternateKey(c => new { c.OrganizationId, c.Id }).HasName("uq_conversation_org");

            builder.HasIndex(c => new { c.OrganizationId, c.Status, c.LastActivityAt, c.Id }).HasDatabaseName("idx_conversation_queue");
            builder.HasIndex(c => new { c.OrganizationId, c.AssignedTo, c.Status, c.LastActivityAt }).HasDatabaseName("idx_conversation_assigned");
            builder.HasIndex(c => new { c.OrganizationId, c.CustomerId, c.CreatedAt }).HasDatabaseName("idx_conversation_customer");

            builder.HasMany(c => c.Participants).WithOne(p => p.Conversation)
                .HasForeignKey(p => new { p.OrganizationId, p.ConversationId })
                .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.Metadata.FindNavigation(nameof(Conversation.Participants))!.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(c => c.Products).WithOne(p => p.Conversation)
                .HasForeignKey(p => new { p.OrganizationId, p.ConversationId })
                .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.Metadata.FindNavigation(nameof(Conversation.Products))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
