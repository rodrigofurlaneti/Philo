using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philo.Domain.Entities;

namespace Philo.Infrastructure.Persistence.Configurations
{
    public sealed class MessageReceiptConfiguration : IEntityTypeConfiguration<MessageReceipt>
    {
        public void Configure(EntityTypeBuilder<MessageReceipt> builder)
        {
            builder.ToTable("message_receipts", t => t.HasCheckConstraint(
                "ck_receipt_read", "read_at IS NULL OR (delivered_at IS NOT NULL AND read_at >= delivered_at)"));

            builder.HasKey(r => new { r.OrganizationId, r.ConversationId, r.MessageId, r.UserId });
            builder.Property(r => r.OrganizationId).HasColumnName("organization_id");
            builder.Property(r => r.ConversationId).HasColumnName("conversation_id");
            builder.Property(r => r.MessageId).HasColumnName("message_id");
            builder.Property(r => r.UserId).HasColumnName("user_id");
            builder.Property(r => r.DeliveredAt).HasColumnName("delivered_at").HasPrecision(6);
            builder.Property(r => r.ReadAt).HasColumnName("read_at").HasPrecision(6);

            builder.HasIndex(r => new { r.OrganizationId, r.UserId, r.ReadAt, r.ConversationId, r.MessageId })
                .HasDatabaseName("idx_receipt_unread");
        }
    }
}
