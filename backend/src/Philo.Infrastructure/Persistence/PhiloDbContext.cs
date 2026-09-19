using Microsoft.EntityFrameworkCore;
using Philo.Domain.Entities;

namespace Philo.Infrastructure.Persistence
{
    /// <summary>
    /// Mapeia exatamente as tabelas criadas por database/schema.sql. Este DbContext não gera
    /// migrations: o schema é instalado uma vez pelo script (README) e o EF Core só mapeia
    /// sobre ele. A trigger trg_message_created e a view vw_message_delivery vivem no banco.
    /// </summary>
    public sealed class PhiloDbContext : DbContext
    {
        public PhiloDbContext(DbContextOptions<PhiloDbContext> options) : base(options) { }

        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<User> Users => Set<User>();
        public DbSet<OrganizationUser> OrganizationUsers => Set<OrganizationUser>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Conversation> Conversations => Set<Conversation>();
        public DbSet<ConversationProduct> ConversationProducts => Set<ConversationProduct>();
        public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>();
        public DbSet<MessageReceipt> MessageReceipts => Set<MessageReceipt>();
        public DbSet<MessageUserPreference> MessageUserPreferences => Set<MessageUserPreference>();
        public DbSet<ConversationEvent> ConversationEvents => Set<ConversationEvent>();
        public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();
        public DbSet<MessageDeliveryView> MessageDeliveryViews => Set<MessageDeliveryView>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PhiloDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
