using Philo.Domain.Entities;
using Philo.Domain.Primitives;

namespace Philo.Application.Common.Services
{
    /// <summary>
    /// Centraliza a checagem repetida em quase todo caso de uso de mensagens: a conversa existe
    /// nesta organização e o usuário é participante ativo dela. As FKs do banco garantem
    /// integridade, mas não autorização — isso é responsabilidade desta camada (README).
    /// </summary>
    public interface IParticipantAccessGuard
    {
        Task<Result<(Conversation Conversation, ConversationParticipant Participant)>> EnsureActiveParticipantAsync(
            long organizationId, long conversationId, long userId, CancellationToken cancellationToken = default);
    }
}
