using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Philo.Application.Common.Interfaces;

namespace Philo.Infrastructure.Services
{
    /// <summary>
    /// Armazenamento privado em disco local com URLs assinadas por HMAC (upload/download).
    /// Implementa o mesmo contrato que uma implementação S3/compatível usaria, então pode ser
    /// substituída sem tocar na Application.
    /// </summary>
    public sealed class LocalDiskFileStorageService : IFileStorageService
    {
        private readonly StorageOptions _options;

        public LocalDiskFileStorageService(IOptions<StorageOptions> options)
        {
            _options = options.Value;
            Directory.CreateDirectory(_options.RootPath);
        }

        public Task<UploadSlot> CreateUploadSlotAsync(string bucket, string suggestedFileName, CancellationToken cancellationToken = default)
        {
            var safeName = SanitizeFileName(suggestedFileName);
            var storageKey = $"{Guid.NewGuid():N}/{safeName}";
            var expiresAt = DateTime.UtcNow.AddMinutes(_options.UploadSlotMinutes);
            var token = SignToken(bucket, storageKey, expiresAt);

            var uploadUrl = $"{_options.PublicBaseUrl.TrimEnd('/')}/api/storage/upload" +
                             $"?bucket={Uri.EscapeDataString(bucket)}&key={Uri.EscapeDataString(storageKey)}" +
                             $"&exp={expiresAt.Ticks}&sig={Uri.EscapeDataString(token)}";

            return Task.FromResult(new UploadSlot(bucket, storageKey, uploadUrl, expiresAt));
        }

        public Task<string> CreateDownloadUrlAsync(string bucket, string storageKey, TimeSpan validFor, CancellationToken cancellationToken = default)
        {
            var expiresAt = DateTime.UtcNow.Add(validFor);
            var token = SignToken(bucket, storageKey, expiresAt);

            var url = $"{_options.PublicBaseUrl.TrimEnd('/')}/api/storage/download" +
                      $"?bucket={Uri.EscapeDataString(bucket)}&key={Uri.EscapeDataString(storageKey)}" +
                      $"&exp={expiresAt.Ticks}&sig={Uri.EscapeDataString(token)}";

            return Task.FromResult(url);
        }

        public async Task<byte[]> ComputeSha256Async(string bucket, string storageKey, CancellationToken cancellationToken = default)
        {
            await using var stream = File.OpenRead(GetPath(bucket, storageKey));
            using var sha256 = SHA256.Create();
            return await sha256.ComputeHashAsync(stream, cancellationToken);
        }

        public Task<long> GetFileSizeAsync(string bucket, string storageKey, CancellationToken cancellationToken = default) =>
            Task.FromResult(new FileInfo(GetPath(bucket, storageKey)).Length);

        public async Task SaveAsync(string bucket, string storageKey, Stream content, CancellationToken cancellationToken = default)
        {
            var path = GetPath(bucket, storageKey);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await using var fileStream = File.Create(path);
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        public Task<Stream> OpenReadAsync(string bucket, string storageKey, CancellationToken cancellationToken = default) =>
            Task.FromResult<Stream>(File.OpenRead(GetPath(bucket, storageKey)));

        public bool TryValidateSignedToken(string bucket, string storageKey, string token, out bool expired)
        {
            expired = false;
            var parts = token.Split('.', 2);
            if (parts.Length != 2 || !long.TryParse(parts[0], out var expTicks))
                return false;

            var expiresAt = new DateTime(expTicks, DateTimeKind.Utc);
            var expected = SignToken(bucket, storageKey, expiresAt);
            if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(token)))
                return false;

            if (expiresAt < DateTime.UtcNow)
            {
                expired = true;
                return false;
            }

            return true;
        }

        private string SignToken(string bucket, string storageKey, DateTime expiresAtUtc)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SigningKey));
            var payload = $"{bucket}:{storageKey}:{expiresAtUtc.Ticks}";
            var hash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
            return $"{expiresAtUtc.Ticks}.{hash}";
        }

        private string GetPath(string bucket, string storageKey)
        {
            var fullPath = Path.GetFullPath(Path.Combine(_options.RootPath, bucket, storageKey));
            var rootFullPath = Path.GetFullPath(Path.Combine(_options.RootPath, bucket));
            if (!fullPath.StartsWith(rootFullPath, StringComparison.Ordinal))
                throw new UnauthorizedAccessException("Chave de armazenamento inválida.");

            return fullPath;
        }

        private static string SanitizeFileName(string fileName)
        {
            var name = Path.GetFileName(fileName);
            foreach (var invalid in Path.GetInvalidFileNameChars())
                name = name.Replace(invalid, '_');

            return string.IsNullOrWhiteSpace(name) ? "arquivo" : name;
        }
    }
}
