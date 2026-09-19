using Microsoft.AspNetCore.SignalR;
using Philo.Application.Common.Interfaces;
using Philo.WebApi.Hubs;

namespace Philo.WebApi.Realtime
{
    public sealed class SignalRRealtimeNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public SignalRRealtimeNotifier(IHubContext<ChatHub> hubContext) => _hubContext = hubContext;

        public Task NotifyConversationAsync(long organizationId, long conversationId, string eventType, object payload, CancellationToken cancellationToken = default) =>
            _hubContext.Clients.Group(ChatHub.GroupName(organizationId, conversationId))
                .SendAsync(eventType, payload, cancellationToken);
    }
}
