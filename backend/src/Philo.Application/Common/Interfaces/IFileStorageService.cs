namespace Philo.Application.Common.Interfaces
{
    public sealed record UploadSlot(string Bucket, string StorageKey, string UploadUrl, DateTime ExpiresAtUtc);

    /// <summary>
    /// Abstrai o armazenamento privado de anexos. A implementação de infraestrutura decide
    /// se as URLs assinadas apontam para disco local, S3 ou compatível.
    /// </summary>
    public interface IFileStorageService
    {
        Task<UploadSlot> CreateUploadSlotAsync(string bucket, string suggestedFileName, CancellationToken cancellationToken = default);
        Task<string> CreateDownloadUrlAsync(string bucket, string storageKey, TimeSpan validFor, CancellationToken cancellationToken = default);
        Task<byte[]> ComputeSha256Async(string bucket, string storageKey, CancellationToken cancellationToken = default);
        Task<long> GetFileSizeAsync(string bucket, string storageKey, CancellationToken cancellationToken = default);

        /// <summary>Grava os bytes recebidos no slot de upload emitido por <see cref="CreateUploadSlotAsync"/>.</summary>
        Task SaveAsync(string bucket, string storageKey, Stream content, CancellationToken cancellationToken = default);

        /// <summary>Abre os bytes já salvos para leitura (usado pelo endpoint de download).</summary>
        Task<Stream> OpenReadAsync(string bucket, string storageKey, CancellationToken cancellationToken = default);

        /// <summary>Valida um token assinado de upload/download emitido por este serviço.</summary>
        bool TryValidateSignedToken(string bucket, string storageKey, string token, out bool expired);
    }
}
