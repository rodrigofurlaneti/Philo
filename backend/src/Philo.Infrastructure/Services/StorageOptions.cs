namespace Philo.Infrastructure.Services
{
    public sealed class StorageOptions
    {
        public const string SectionName = "Storage";

        /// <summary>Diretório local privado onde os arquivos ficam (fora do wwwroot público).</summary>
        public string RootPath { get; set; } = "storage-private";

        /// <summary>Base pública usada para montar as URLs assinadas de upload/download.</summary>
        public string PublicBaseUrl { get; set; } = "http://localhost:5000";

        public string SigningKey { get; set; } = string.Empty;

        public int UploadSlotMinutes { get; set; } = 15;
    }
}
