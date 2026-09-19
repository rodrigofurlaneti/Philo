using Philo.Application.Common.Interfaces;

namespace Philo.Infrastructure.Services
{
    /// <summary>
    /// Verificação padrão (sem antivírus real): rejeita tipos executáveis conhecidos e
    /// arquivos fora do limite de tamanho. Pode ser substituída por um scanner real via DI
    /// sem alterar a Application.
    /// </summary>
    public sealed class BasicAttachmentScanner : IAttachmentScanner
    {
        private static readonly HashSet<string> BlockedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/x-msdownload",
            "application/x-executable",
            "application/x-sh",
            "application/x-bat",
            "application/vnd.microsoft.portable-executable",
        };

        public Task<ScanVerdict> ScanAsync(string mimeType, long fileSizeBytes, byte[] sha256, CancellationToken cancellationToken = default)
        {
            var verdict = BlockedMimeTypes.Contains(mimeType) || fileSizeBytes <= 0
                ? ScanVerdict.Rejected
                : ScanVerdict.Clean;

            return Task.FromResult(verdict);
        }
    }
}
