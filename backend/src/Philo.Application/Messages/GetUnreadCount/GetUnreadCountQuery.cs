using MediatR;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Messages.GetUnreadCount
{
    public sealed record GetUnreadCountQuery(long OrganizationId, long ConversationId, long UserId) : IRequest<Result<int>>;

    public sealed class GetUnreadCountHandler : IRequestHandler<GetUnreadCountQuery, Result<int>>
    {
        private readonly IMessageRepository _messages;

        public GetUnreadCountHandler(IMessageRepository messages) => _messages = messages;

        public async Task<Result<int>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
        {
            var count = await _messages.GetUnreadCountAsync(request.OrganizationId, request.ConversationId, request.UserId, cancellationToken);
            return Result.Success(count);
        }
    }
}
