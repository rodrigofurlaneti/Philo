using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philo.Domain.Entities;

namespace Philo.Infrastructure.Persistence.Configurations
{
    public sealed class ConversationEventConfiguration : IEntityTypeConfiguration<ConversationEvent>
    {
        public void Configure(EntityTypeBuilder<ConversationEvent> builder)
        {
            builder.ToTable("conversation_events");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(e => e.OrganizationId).HasColumnName("organization_id");
            builder.Property(e => e.ConversationId).HasColumnName("conversation_id");
            builder.Property(e => e.ActorId).HasColumnName("actor_id");
            builder.Property(e => e.EventType).HasColumnName("event_type").HasConversion(EnumConverters.ConversationEventTypeConverter).HasMaxLength(30).IsRequired();
            builder.Property(e => e.Details).HasColumnName("details").HasColumnType("json").IsRequired();
            builder.Property(e => e.CreatedAt).HasColumnName("created_at").HasPrecision(6);

            builder.HasIndex(e => new { e.OrganizationId, e.ConversationId, e.CreatedAt, e.Id }).HasDatabaseName("idx_event_history");
        }
    }
}
