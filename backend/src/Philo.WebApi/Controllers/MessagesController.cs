using MediatR;
using Microsoft.AspNetCore.Mvc;
using Philo.Application.Common.Interfaces;
using Philo.Application.Messages.Common;
using Philo.Application.Messages.DeleteMessage;
using Philo.Application.Messages.EditMessage;
using Philo.Application.Messages.GetHistory;
using Philo.Application.Messages.GetUnreadCount;
using Philo.Application.Messages.MarkDelivered;
using Philo.Application.Messages.MarkRead;
using Philo.Application.Messages.SendAttachmentMessage;
using Philo.Application.Messages.SendTextMessage;
using Philo.WebApi.Common;

namespace Philo.WebApi.Controllers
{
    [Route("api/organizations/{organizationId:long}/conversations/{conversationId:long}/messages")]
    public sealed class MessagesController : PhiloControllerBase
    {
        public MessagesController(ISender mediator, ICurrentUserService currentUser) : base(mediator, currentUser) { }

        public sealed record SendTextRequest(Guid ClientMessageId, string Body, long? ReplyToId, DateTime? ExpiresAt);
        public sealed record SendAttachmentRequest(Guid ClientMessageId, string? Caption, long? ReplyToId, DateTime? ExpiresAt, IReadOnlyList<AttachmentInput> Attachments);
        public sealed record EditMessageRequest(string Body);

        /// <summary>Histórico paginado por cursor (sent_at desc, id desc).</summary>
        [HttpGet]
        public async Task<IActionResult> GetHistory(
            long organizationId, long conversationId,
            [FromQuery] DateTime? cursorSentAt, [FromQuery] long? cursorId, [FromQuery] int limit = 50,
            CancellationToken cancellationToken = default)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;

            var result = await Mediator.Send(
                new GetHistoryQuery(organizationId, conversationId, CurrentUser.UserId, cursorSentAt, cursorId, limit), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount(long organizationId, long conversationId, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;

            var result = await Mediator.Send(new GetUnreadCountQuery(organizationId, conversationId, CurrentUser.UserId), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPost("text")]
        public async Task<IActionResult> SendText(long organizationId, long conversationId, SendTextRequest request, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;

            var command = new SendTextMessageCommand(
                organizationId, conversationId, CurrentUser.UserId, request.ClientMessageId, request.Body, request.ReplyToId, request.ExpiresAt);
            var result = await Mediator.Send(command, cancellationToken);
            return result.ToActionResult(this, id => CreatedAtAction(nameof(GetHistory), new { organizationId, conversationId }, new { id }));
        }

        [HttpPost("attachment")]
        public async Task<IActionResult> SendAttachment(long organizationId, long conversationId, SendAttachmentRequest request, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;

            var command = new SendAttachmentMessageCommand(
                organizationId, conversationId, CurrentUser.UserId, request.ClientMessageId,
                request.Caption, request.ReplyToId, request.ExpiresAt, request.Attachments);
            var result = await Mediator.Send(command, cancellationToken);
            return result.ToActionResult(this, id => CreatedAtAction(nameof(GetHistory), new { organizationId, conversationId }, new { id }));
        }

        [HttpPut("{messageId:long}")]
        public async Task<IActionResult> Edit(long organizationId, long conversationId, long messageId, EditMessageRequest request, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;

            var result = await Mediator.Send(new EditMessageCommand(organizationId, conversationId, messageId, CurrentUser.UserId, request.Body), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpDelete("{messageId:long}")]
        public async Task<IActionResult> Delete(long organizationId, long conversationId, long messageId, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;

            var result = await Mediator.Send(new DeleteMessageCommand(organizationId, conversationId, messageId, CurrentUser.UserId), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPost("{messageId:long}/delivered")]
        public async Task<IActionResult> MarkDelivered(long organizationId, long conversationId, long messageId, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;

            var result = await Mediator.Send(new MarkDeliveredCommand(organizationId, conversationId, messageId, CurrentUser.UserId), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPost("{messageId:long}/read")]
        public async Task<IActionResult> MarkRead(long organizationId, long conversationId, long messageId, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;

            var result = await Mediator.Send(new MarkReadCommand(organizationId, conversationId, messageId, CurrentUser.UserId), cancellationToken);
            return result.ToActionResult(this);
        }
    }
}
