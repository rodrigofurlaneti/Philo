namespace Philo.Infrastructure.Services
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = "Philo";
        public string Audience { get; set; } = "Philo.Clients";
        public string SigningKey { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 60;
    }
}
