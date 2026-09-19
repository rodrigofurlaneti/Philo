using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philo.Domain.Entities;

namespace Philo.Infrastructure.Persistence.Configurations
{
    public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("messages", t =>
            {
                t.HasCheckConstraint("ck_message_body",
                    "deleted_at IS NOT NULL OR message_type = 'attachment' OR (body IS NOT NULL AND CHAR_LENGTH(TRIM(body)) > 0)");
                t.HasCheckConstraint("ck_message_deleted", "deleted_at IS NULL OR (body IS NULL AND deleted_at >= sent_at)");
                t.HasCheckConstraint("ck_message_edited", "edited_at IS NULL OR edited_at >= sent_at");
                t.HasCheckConstraint("ck_message_expiry", "expires_at IS NULL OR expires_at > sent_at");
            });

            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(m => m.OrganizationId).HasColumnName("organization_id");
            builder.Property(m => m.ConversationId).HasColumnName("conversation_id");
            builder.Property(m => m.SenderId).HasColumnName("sender_id");
            builder.Property(m => m.ClientMessageId).HasColumnName("client_message_id")
                .HasConversion(g => g.ToString(), s => Guid.Parse(s)).HasMaxLength(36).IsRequired();
            builder.Property(m => m.MessageType).HasColumnName("message_type").HasConversion(EnumConverters.MessageTypeConverter).HasMaxLength(20).IsRequired();
            builder.Property(m => m.Body).HasColumnName("body").HasColumnType("text");
            builder.Property(m => m.ReplyToId).HasColumnName("reply_to_id");
            builder.Property(m => m.SentAt).HasColumnName("sent_at").HasPrecision(6);
            builder.Property(m => m.EditedAt).HasColumnName("edited_at").HasPrecision(6);
            builder.Property(m => m.DeletedAt).HasColumnName("deleted_at").HasPrecision(6);
            builder.Property(m => m.ExpiresAt).HasColumnName("expires_at").HasPrecision(6);

            builder.HasAlternateKey(m => new { m.OrganizationId, m.ConversationId, m.Id }).HasName("uq_message_scope");
            builder.HasIndex(m => new { m.OrganizationId, m.ConversationId, m.SenderId, m.ClientMessageId })
                .IsUnique().HasDatabaseName("uq_message_retry");
            builder.HasIndex(m => new { m.OrganizationId, m.ConversationId, m.SentAt, m.Id }).HasDatabaseName("idx_message_history");
            builder.HasIndex(m => new { m.ExpiresAt, m.Id }).HasDatabaseName("idx_message_expiry");

            builder.HasOne(m => m.Conversation).WithMany()
                .HasForeignKey(m => new { m.OrganizationId, m.ConversationId })
                .HasPrincipalKey(c => new { c.OrganizationId, c.Id })
                .OnDelete(DeleteBehavior.Restrict);

            // sender_id referencia conversation_participants(organization_id, conversation_id, user_id),
            // não users diretamente — a navegação Sender é apenas de conveniência de leitura (join manual).
            builder.Ignore(m => m.Sender);

            // Auto-relacionamento reply_to_id -> mesma conversa/empresa.
            builder.HasOne<Message>().WithMany()
                .HasForeignKey(m => new { m.OrganizationId, m.ConversationId, m.ReplyToId })
                .HasPrincipalKey(m => new { m.OrganizationId, m.ConversationId, m.Id })
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasMany(m => m.Attachments).WithOne()
                .HasForeignKey(a => new { a.OrganizationId, a.ConversationId, a.MessageId })
                .HasPrincipalKey(m => new { m.OrganizationId, m.ConversationId, m.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.Metadata.FindNavigation(nameof(Message.Attachments))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
