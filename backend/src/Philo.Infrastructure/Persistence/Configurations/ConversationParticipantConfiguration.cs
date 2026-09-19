using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Philo.Domain.Entities;

namespace Philo.Infrastructure.Persistence.Configurations
{
    public sealed class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
    {
        public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
        {
            builder.ToTable("conversation_participants", t => t.HasCheckConstraint(
                "ck_participant_dates", "left_at IS NULL OR left_at >= joined_at"));

            builder.HasKey(p => new { p.OrganizationId, p.ConversationId, p.UserId });
            builder.Property(p => p.OrganizationId).HasColumnName("organization_id");
            builder.Property(p => p.ConversationId).HasColumnName("conversation_id");
            builder.Property(p => p.UserId).HasColumnName("user_id");
            builder.Property(p => p.JoinedAt).HasColumnName("joined_at").HasPrecision(6);
            builder.Property(p => p.LeftAt).HasColumnName("left_at").HasPrecision(6);
            builder.Property(p => p.ArchivedAt).HasColumnName("archived_at").HasPrecision(6);
            builder.Property(p => p.PinnedAt).HasColumnName("pinned_at").HasPrecision(6);
            builder.Property(p => p.MutedUntil).HasColumnName("muted_until").HasPrecision(6);

            builder.HasIndex(p => new { p.OrganizationId, p.UserId, p.LeftAt, p.ArchivedAt, p.ConversationId })
                .HasDatabaseName("idx_participant_inbox");

            // user_id referencia organization_users(organization_id, user_id), não users diretamente —
            // a navegação User é apenas de conveniência de leitura (join manual via IUserRepository).
            builder.Ignore(p => p.User);
        }
    }
}
