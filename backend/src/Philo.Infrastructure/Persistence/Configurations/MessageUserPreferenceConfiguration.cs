using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philo.Domain.Entities;

namespace Philo.Infrastructure.Persistence.Configurations
{
    public sealed class MessageUserPreferenceConfiguration : IEntityTypeConfiguration<MessageUserPreference>
    {
        public void Configure(EntityTypeBuilder<MessageUserPreference> builder)
        {
            builder.ToTable("message_user_preferences");
            builder.HasKey(p => new { p.OrganizationId, p.ConversationId, p.MessageId, p.UserId });
            builder.Property(p => p.OrganizationId).HasColumnName("organization_id");
            builder.Property(p => p.ConversationId).HasColumnName("conversation_id");
            builder.Property(p => p.MessageId).HasColumnName("message_id");
            builder.Property(p => p.UserId).HasColumnName("user_id");
            builder.Property(p => p.StarredAt).HasColumnName("starred_at").HasPrecision(6);
            builder.Property(p => p.HiddenAt).HasColumnName("hidden_at").HasPrecision(6);

            builder.HasIndex(p => new { p.OrganizationId, p.UserId, p.StarredAt }).HasDatabaseName("idx_preference_user");
        }
    }
}
