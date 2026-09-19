namespace Philo.WebApi.Auth
{
    public sealed class InternalApiKeyOptions
    {
        public const string SectionName = "InternalApi";

        /// <summary>Segredo compartilhado com o backend do site para emitir sessões (ver README ponto 2).</summary>
        public string ApiKey { get; set; } = string.Empty;
    }
}
