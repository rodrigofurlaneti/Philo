using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philo.Domain.Entities;

namespace Philo.Infrastructure.Persistence.Configurations
{
    public sealed class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
    {
        public void Configure(EntityTypeBuilder<OutboxEvent> builder)
        {
            builder.ToTable("outbox_events");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(o => o.OrganizationId).HasColumnName("organization_id");
            builder.Property(o => o.ConversationId).HasColumnName("conversation_id");
            builder.Property(o => o.MessageId).HasColumnName("message_id");
            builder.Property(o => o.EventType).HasColumnName("event_type").HasMaxLength(64).IsRequired();
            builder.Property(o => o.CreatedAt).HasColumnName("created_at").HasPrecision(6);
            builder.Property(o => o.AvailableAt).HasColumnName("available_at").HasPrecision(6);
            builder.Property(o => o.PublishedAt).HasColumnName("published_at").HasPrecision(6);
            builder.Property(o => o.Attempts).HasColumnName("attempts");

            builder.HasIndex(o => new { o.PublishedAt, o.AvailableAt, o.Id }).HasDatabaseName("idx_outbox_pending");
        }
    }
}
