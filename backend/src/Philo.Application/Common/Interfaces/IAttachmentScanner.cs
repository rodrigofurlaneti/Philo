namespace Philo.Application.Common.Interfaces
{
    public enum ScanVerdict { Clean, Rejected }

    /// <summary>
    /// Verificação de segurança do arquivo antes de liberar o download (scan_status).
    /// A implementação padrão de infraestrutura faz uma checagem básica de MIME/tamanho;
    /// pode ser substituída por um antivírus real sem alterar a Application.
    /// </summary>
    public interface IAttachmentScanner
    {
        Task<ScanVerdict> ScanAsync(string mimeType, long fileSizeBytes, byte[] sha256, CancellationToken cancellationToken = default);
    }
}
