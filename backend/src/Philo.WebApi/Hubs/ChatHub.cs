using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Philo.Application.Common.Interfaces;
using Philo.Domain.Interfaces;

namespace Philo.WebApi.Hubs
{
    /// <summary>
    /// Canal em tempo real. Ingressar em um grupo de conversa exige ser participante ativo dela
    /// (mesma autorização usada pelos endpoints HTTP) — a inscrição no WebSocket também precisa
    /// ser autorizada (README ponto 5).
    /// </summary>
    [Authorize]
    public sealed class ChatHub : Hub
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IConversationParticipantRepository _participants;

        public ChatHub(ICurrentUserService currentUser, IConversationParticipantRepository participants)
        {
            _currentUser = currentUser;
            _participants = participants;
        }

        public async Task JoinConversation(long conversationId)
        {
            var isActive = await _participants.IsActiveParticipantAsync(_currentUser.OrganizationId, conversationId, _currentUser.UserId);
            if (!isActive)
                throw new HubException("Acesso negado a esta conversa.");

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(_currentUser.OrganizationId, conversationId));
        }

        public async Task LeaveConversation(long conversationId) =>
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(_currentUser.OrganizationId, conversationId));

        internal static string GroupName(long organizationId, long conversationId) => $"org:{organizationId}:conv:{conversationId}";
    }
}
