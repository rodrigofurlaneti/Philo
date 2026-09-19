using MediatR;
using Microsoft.AspNetCore.Mvc;
using Philo.Application.Common.Interfaces;
using Philo.Application.Conversations.AddParticipant;
using Philo.Application.Conversations.AssignConversation;
using Philo.Application.Conversations.ChangeConversationStatus;
using Philo.Application.Conversations.CloseConversation;
using Philo.Application.Conversations.GetConversationDetails;
using Philo.Application.Conversations.GetMyConversations;
using Philo.Application.Conversations.GetQueue;
using Philo.Application.Conversations.LinkProduct;
using Philo.Application.Conversations.OpenConversation;
using Philo.Application.Conversations.RemoveParticipant;
using Philo.Application.Conversations.ReopenConversation;
using Philo.Domain.Enums;
using Philo.WebApi.Common;

namespace Philo.WebApi.Controllers
{
    [Route("api/organizations/{organizationId:long}/conversations")]
    public sealed class ConversationsController : PhiloControllerBase
    {
        public ConversationsController(ISender mediator, ICurrentUserService currentUser) : base(mediator, currentUser) { }

        public sealed record OpenConversationRequest(long CustomerId, ConversationPurpose Purpose, string? Subject, Priority Priority, string? SourcePageUrl);
        public sealed record AssignRequest(long AgentId);
        public sealed record ChangeStatusRequest(ConversationStatus Status);
        public sealed record ParticipantRequest(long UserId);
        public sealed record LinkProductRequest(long ProductId);

        /// <summary>Fila da equipe (queries.sql #7). Restrito a agent/admin.</summary>
        [HttpGet("queue")]
        public async Task<IActionResult> GetQueue(long organizationId, [FromQuery] string? status, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;
            if (!IsStaff) return Forbid();

            var result = await Mediator.Send(new GetQueueQuery(organizationId, status, limit), cancellationToken);
            return result.ToActionResult(this);
        }

        /// <summary>Conversas do próprio cliente autenticado.</summary>
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine(long organizationId, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;

            var result = await Mediator.Send(new GetMyConversationsQuery(organizationId, CurrentUser.UserId, limit), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpGet("{conversationId:long}")]
        public async Task<IActionResult> GetDetails(long organizationId, long conversationId, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;

            var result = await Mediator.Send(new GetConversationDetailsQuery(organizationId, conversationId), cancellationToken);
            return result.ToActionResult(this);
        }

        /// <summary>Uma conversa deve começar com o cliente como participante (README).</summary>
        [HttpPost]
        public async Task<IActionResult> Open(long organizationId, OpenConversationRequest request, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;

            var command = new OpenConversationCommand(
                organizationId, request.CustomerId, CurrentUser.UserId, request.Purpose, request.Subject, request.Priority, request.SourcePageUrl);
            var result = await Mediator.Send(command, cancellationToken);
            return result.ToActionResult(this, id => CreatedAtAction(nameof(GetDetails), new { organizationId, conversationId = id }, new { id }));
        }

        [HttpPost("{conversationId:long}/assign")]
        public async Task<IActionResult> Assign(long organizationId, long conversationId, AssignRequest request, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;
            if (!IsStaff) return Forbid();

            var result = await Mediator.Send(new AssignConversationCommand(organizationId, conversationId, request.AgentId, CurrentUser.UserId), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPost("{conversationId:long}/status")]
        public async Task<IActionResult> ChangeStatus(long organizationId, long conversationId, ChangeStatusRequest request, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;
            if (!IsStaff) return Forbid();

            var result = await Mediator.Send(new ChangeConversationStatusCommand(organizationId, conversationId, request.Status, CurrentUser.UserId), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPost("{conversationId:long}/close")]
        public async Task<IActionResult> Close(long organizationId, long conversationId, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;
            if (!IsStaff) return Forbid();

            var result = await Mediator.Send(new CloseConversationCommand(organizationId, conversationId, CurrentUser.UserId), cancellationToken);
            return result.ToActionResult(this);
        }

        /// <summary>A política de reabertura é explícita e restrita à equipe (README).</summary>
        [HttpPost("{conversationId:long}/reopen")]
        public async Task<IActionResult> Reopen(long organizationId, long conversationId, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;
            if (!IsStaff) return Forbid();

            var result = await Mediator.Send(new ReopenConversationCommand(organizationId, conversationId, CurrentUser.UserId), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPost("{conversationId:long}/participants")]
        public async Task<IActionResult> AddParticipant(long organizationId, long conversationId, ParticipantRequest request, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;
            if (!IsStaff) return Forbid();

            var result = await Mediator.Send(new AddParticipantCommand(organizationId, conversationId, request.UserId, CurrentUser.UserId), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpDelete("{conversationId:long}/participants/{userId:long}")]
        public async Task<IActionResult> RemoveParticipant(long organizationId, long conversationId, long userId, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;
            if (!IsStaff) return Forbid();

            var result = await Mediator.Send(new RemoveParticipantCommand(organizationId, conversationId, userId, CurrentUser.UserId), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPost("{conversationId:long}/products")]
        public async Task<IActionResult> LinkProduct(long organizationId, long conversationId, LinkProductRequest request, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;

            var result = await Mediator.Send(new LinkProductCommand(organizationId, conversationId, request.ProductId), cancellationToken);
            return result.ToActionResult(this);
        }
    }
}
