using Philo.Application.Common.Interfaces;

namespace Philo.Infrastructure.Services
{
    public sealed class SystemDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
