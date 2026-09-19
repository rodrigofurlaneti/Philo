using FluentValidation;
using MediatR;
using Philo.Application.Common.Services;
using Philo.Application.Messages.Common;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Messages.GetHistory
{
    /// <summary>
    /// Histórico paginado por cursor (sent_at, id) para não pular/repetir linhas com timestamps
    /// iguais (validation.md). Mensagens expiradas e ocultadas pelo usuário já vêm filtradas do repositório.
    /// </summary>
    public sealed record GetHistoryQuery(
        long OrganizationId, long ConversationId, long RequestingUserId,
        DateTime? CursorSentAt, long? CursorId, int Limit = 50) : IRequest<Result<IReadOnlyList<MessageDto>>>;

    public sealed class GetHistoryValidator : AbstractValidator<GetHistoryQuery>
    {
        public GetHistoryValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.RequestingUserId).GreaterThan(0);
        }
    }

    public sealed class GetHistoryHandler : IRequestHandler<GetHistoryQuery, Result<IReadOnlyList<MessageDto>>>
    {
        private readonly IParticipantAccessGuard _accessGuard;
        private readonly IMessageRepository _messages;
        private readonly IMessageUserPreferenceRepository _preferences;

        public GetHistoryHandler(IParticipantAccessGuard accessGuard, IMessageRepository messages, IMessageUserPreferenceRepository preferences)
        {
            _accessGuard = accessGuard;
            _messages = messages;
            _preferences = preferences;
        }

        public async Task<Result<IReadOnlyList<MessageDto>>> Handle(GetHistoryQuery request, CancellationToken cancellationToken)
        {
            var access = await _accessGuard.EnsureActiveParticipantAsync(request.OrganizationId, request.ConversationId, request.RequestingUserId, cancellationToken);
            if (access.IsFailure)
                return Result.Failure<IReadOnlyList<MessageDto>>(access.Error);

            var limit = request.Limit is > 0 and <= 100 ? request.Limit : 50;
            var messages = await _messages.GetHistoryAsync(
                request.OrganizationId, request.ConversationId, request.RequestingUserId,
                request.CursorSentAt, request.CursorId, limit, cancellationToken);

            var dtos = new List<MessageDto>(messages.Count);
            foreach (var message in messages)
            {
                var preference = await _preferences.GetAsync(request.OrganizationId, request.ConversationId, message.Id, request.RequestingUserId, cancellationToken);
                dtos.Add(new MessageDto(
                    message.Id, message.SenderId, message.MessageType.ToString(),
                    message.IsDeleted ? null : message.Body,
                    message.ReplyToId, message.SentAt, message.EditedAt, message.IsDeleted,
                    preference?.StarredAt is not null));
            }

            return Result.Success<IReadOnlyList<MessageDto>>(dtos);
        }
    }
}
