using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philo.Domain.Entities;

namespace Philo.Infrastructure.Persistence.Configurations
{
    public sealed class MessageAttachmentConfiguration : IEntityTypeConfiguration<MessageAttachment>
    {
        public void Configure(EntityTypeBuilder<MessageAttachment> builder)
        {
            builder.ToTable("message_attachments", t => t.HasCheckConstraint(
                "ck_attachment_size", $"file_size_bytes > 0 AND file_size_bytes <= {MessageAttachment.MaxFileSizeBytes}"));

            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(a => a.OrganizationId).HasColumnName("organization_id");
            builder.Property(a => a.ConversationId).HasColumnName("conversation_id");
            builder.Property(a => a.MessageId).HasColumnName("message_id");
            builder.Property(a => a.OriginalName).HasColumnName("original_name").HasMaxLength(255).IsRequired();
            builder.Property(a => a.MimeType).HasColumnName("mime_type").HasMaxLength(127).IsRequired();
            builder.Property(a => a.FileSizeBytes).HasColumnName("file_size_bytes");
            builder.Property(a => a.StorageBucket).HasColumnName("storage_bucket").HasMaxLength(63).IsRequired();
            builder.Property(a => a.StorageKey).HasColumnName("storage_key").HasMaxLength(512).IsRequired();
            builder.Property(a => a.Sha256).HasColumnName("sha256").HasMaxLength(32).IsFixedLength().IsRequired();
            builder.Property(a => a.ScanStatus).HasColumnName("scan_status").HasConversion(EnumConverters.ScanStatusConverter).HasMaxLength(20).IsRequired();
            builder.Property(a => a.CreatedAt).HasColumnName("created_at").HasPrecision(6);

            builder.HasIndex(a => new { a.StorageBucket, a.StorageKey }).IsUnique().HasDatabaseName("uq_attachment_object");
            builder.HasIndex(a => new { a.OrganizationId, a.ConversationId, a.MessageId }).HasDatabaseName("idx_attachment_message");
        }
    }
}
