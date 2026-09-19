using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Philo.Application.Common.Interfaces;

namespace Philo.WebApi.Controllers
{
    /// <summary>
    /// Implementação local (disco privado) do "armazenamento privado" exigido pelo README.
    /// O acesso não depende do JWT do chat, e sim do token HMAC de curta duração embutido na
    /// URL assinada — a mesma URL que só é emitida depois de autorizar o participante.
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("api/storage")]
    public sealed class StorageController : ControllerBase
    {
        private readonly IFileStorageService _storage;

        public StorageController(IFileStorageService storage) => _storage = storage;

        [HttpPut("upload")]
        [RequestSizeLimit(104_857_600)]
        public async Task<IActionResult> Upload([FromQuery] string bucket, [FromQuery] string key, [FromQuery] string sig, CancellationToken cancellationToken)
        {
            if (!_storage.TryValidateSignedToken(bucket, key, sig, out var expired))
                return expired ? Problem(title: "Upload.Expired", statusCode: StatusCodes.Status410Gone) : Forbid();

            await _storage.SaveAsync(bucket, key, Request.Body, cancellationToken);
            return NoContent();
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] string bucket, [FromQuery] string key, [FromQuery] string sig, CancellationToken cancellationToken)
        {
            if (!_storage.TryValidateSignedToken(bucket, key, sig, out var expired))
                return expired ? Problem(title: "Download.Expired", statusCode: StatusCodes.Status410Gone) : Forbid();

            var stream = await _storage.OpenReadAsync(bucket, key, cancellationToken);
            return File(stream, "application/octet-stream");
        }
    }
}
