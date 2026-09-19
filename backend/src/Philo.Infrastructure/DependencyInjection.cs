using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Philo.Application.Common.Interfaces;
using Philo.Domain.Interfaces;
using Philo.Infrastructure.BackgroundServices;
using Philo.Infrastructure.Persistence;
using Philo.Infrastructure.Persistence.Repositories;
using Philo.Infrastructure.Services;

namespace Philo.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("PhiloDb")
                ?? throw new InvalidOperationException("Connection string 'PhiloDb' não configurada.");

            services.AddDbContext<PhiloDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure(3)));

            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
            services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));

            services.AddHttpContextAccessor();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOrganizationUserRepository, OrganizationUserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IConversationParticipantRepository, ConversationParticipantRepository>();
            services.AddScoped<IConversationProductRepository, ConversationProductRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IMessageAttachmentRepository, MessageAttachmentRepository>();
            services.AddScoped<IMessageReceiptRepository, MessageReceiptRepository>();
            services.AddScoped<IMessageUserPreferenceRepository, MessageUserPreferenceRepository>();
            services.AddScoped<IConversationEventRepository, ConversationEventRepository>();
            services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
            services.AddSingleton<IAttachmentScanner, BasicAttachmentScanner>();
            services.AddSingleton<IFileStorageService, LocalDiskFileStorageService>();

            services.AddHostedService<OutboxPublisherService>();

            return services;
        }
    }
}
