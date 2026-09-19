using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Philo.Infrastructure.Persistence.Configurations
{
    public sealed class MessageDeliveryViewConfiguration : IEntityTypeConfiguration<MessageDeliveryView>
    {
        public void Configure(EntityTypeBuilder<MessageDeliveryView> builder)
        {
            builder.ToView("vw_message_delivery");
            builder.HasNoKey();
            builder.Property(v => v.OrganizationId).HasColumnName("organization_id");
            builder.Property(v => v.ConversationId).HasColumnName("conversation_id");
            builder.Property(v => v.MessageId).HasColumnName("message_id");
            builder.Property(v => v.ExpectedRecipients).HasColumnName("expected_recipients");
            builder.Property(v => v.DeliveredCount).HasColumnName("delivered_count");
            builder.Property(v => v.ReadCount).HasColumnName("read_count");
            builder.Property(v => v.DeliveryStatus).HasColumnName("delivery_status").HasMaxLength(20);
        }
    }
}
