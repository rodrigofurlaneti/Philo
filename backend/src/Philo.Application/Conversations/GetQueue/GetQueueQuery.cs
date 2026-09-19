using MediatR;
using Philo.Application.Conversations.Common;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Conversations.GetQueue
{
    /// <summary>Fila da equipe (queries.sql #7). Endpoint restrito a agent/admin da empresa.</summary>
    public sealed record GetQueueQuery(long OrganizationId, string? Status, int Limit = 50) : IRequest<Result<IReadOnlyList<ConversationSummaryDto>>>;

    public sealed class GetQueueHandler : IRequestHandler<GetQueueQuery, Result<IReadOnlyList<ConversationSummaryDto>>>
    {
        private readonly IConversationRepository _conversations;

        public GetQueueHandler(IConversationRepository conversations) => _conversations = conversations;

        public async Task<Result<IReadOnlyList<ConversationSummaryDto>>> Handle(GetQueueQuery request, CancellationToken cancellationToken)
        {
            var limit = request.Limit is > 0 and <= 200 ? request.Limit : 50;
            var conversations = await _conversations.GetQueueAsync(request.OrganizationId, request.Status, limit, cancellationToken);

            IReadOnlyList<ConversationSummaryDto> dtos = conversations
                .Select(c => new ConversationSummaryDto(c.Id, c.CustomerId, c.AssignedTo, c.Purpose.ToString(), c.Subject, c.Status.ToString(), c.Priority.ToString(), c.LastActivityAt))
                .ToList();

            return Result.Success(dtos);
        }
    }
}
