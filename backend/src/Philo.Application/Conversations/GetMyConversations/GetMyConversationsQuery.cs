using MediatR;
using Philo.Application.Conversations.Common;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Conversations.GetMyConversations
{
    /// <summary>Conversas do próprio cliente autenticado.</summary>
    public sealed record GetMyConversationsQuery(long OrganizationId, long CustomerId, int Limit = 50) : IRequest<Result<IReadOnlyList<ConversationSummaryDto>>>;

    public sealed class GetMyConversationsHandler : IRequestHandler<GetMyConversationsQuery, Result<IReadOnlyList<ConversationSummaryDto>>>
    {
        private readonly IConversationRepository _conversations;

        public GetMyConversationsHandler(IConversationRepository conversations) => _conversations = conversations;

        public async Task<Result<IReadOnlyList<ConversationSummaryDto>>> Handle(GetMyConversationsQuery request, CancellationToken cancellationToken)
        {
            var limit = request.Limit is > 0 and <= 200 ? request.Limit : 50;
            var conversations = await _conversations.GetForCustomerAsync(request.OrganizationId, request.CustomerId, limit, cancellationToken);

            IReadOnlyList<ConversationSummaryDto> dtos = conversations
                .Select(c => new ConversationSummaryDto(c.Id, c.CustomerId, c.AssignedTo, c.Purpose.ToString(), c.Subject, c.Status.ToString(), c.Priority.ToString(), c.LastActivityAt))
                .ToList();

            return Result.Success(dtos);
        }
    }
}
