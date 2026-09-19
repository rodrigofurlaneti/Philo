using MediatR;
using Microsoft.AspNetCore.Mvc;
using Philo.Application.Attachments.GetDownloadUrl;
using Philo.Application.Attachments.RequestUploadSlot;
using Philo.Application.Common.Interfaces;
using Philo.WebApi.Common;

namespace Philo.WebApi.Controllers
{
    [Route("api/organizations/{organizationId:long}/conversations/{conversationId:long}/attachments")]
    public sealed class AttachmentsController : PhiloControllerBase
    {
        public AttachmentsController(ISender mediator, ICurrentUserService currentUser) : base(mediator, currentUser) { }

        public sealed record UploadSlotRequest(string FileName);

        /// <summary>Passo 1: obter URL temporária de upload direto ao armazenamento privado.</summary>
        [HttpPost("upload-slot")]
        public async Task<IActionResult> RequestUploadSlot(long organizationId, long conversationId, UploadSlotRequest request, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;

            var result = await Mediator.Send(new RequestUploadSlotCommand(organizationId, conversationId, CurrentUser.UserId, request.FileName), cancellationToken);
            return result.ToActionResult(this);
        }

        /// <summary>Passo 3: obter URL temporária de download, já autorizada (nunca a chave interna direto).</summary>
        [HttpGet("{attachmentId:long}/download-url")]
        public async Task<IActionResult> GetDownloadUrl(long organizationId, long conversationId, long attachmentId, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;

            var result = await Mediator.Send(new GetDownloadUrlQuery(organizationId, conversationId, attachmentId, CurrentUser.UserId), cancellationToken);
            return result.ToActionResult(this, url => Ok(new { url }));
        }
    }
}
