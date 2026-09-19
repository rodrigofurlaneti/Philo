using MediatR;
using Philo.Application.Conversations.Common;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Conversations.GetConversationDetails
{
    /// <summary>
    /// Detalhe de uma conversa. O chamador (controller) já garantiu que o usuário autenticado
    /// é participante ativo ou tem papel de equipe autorizado a ver a fila.
    /// </summary>
    public sealed record GetConversationDetailsQuery(long OrganizationId, long ConversationId) : IRequest<Result<ConversationDetailsDto>>;

    public sealed class GetConversationDetailsHandler : IRequestHandler<GetConversationDetailsQuery, Result<ConversationDetailsDto>>
    {
        private readonly IConversationRepository _conversations;

        public GetConversationDetailsHandler(IConversationRepository conversations) => _conversations = conversations;

        public async Task<Result<ConversationDetailsDto>> Handle(GetConversationDetailsQuery request, CancellationToken cancellationToken)
        {
            var conversation = await _conversations.GetWithParticipantsAsync(request.OrganizationId, request.ConversationId, cancellationToken);
            if (conversation is null)
                return Result.Failure<ConversationDetailsDto>(DomainErrors.Conversation.NotFound);

            var participants = conversation.Participants
                .Select(p => new ConversationParticipantDto(p.UserId, p.JoinedAt, p.LeftAt, p.IsActive))
                .ToList();

            var dto = new ConversationDetailsDto(
                conversation.Id, conversation.OrganizationId, conversation.CustomerId, conversation.AssignedTo, conversation.CreatedBy,
                conversation.Purpose.ToString(), conversation.Subject, conversation.Status.ToString(), conversation.Priority.ToString(),
                conversation.SourcePageUrl, conversation.LastActivityAt, conversation.CreatedAt, conversation.ClosedAt, participants);

            return Result.Success(dto);
        }
    }
}
