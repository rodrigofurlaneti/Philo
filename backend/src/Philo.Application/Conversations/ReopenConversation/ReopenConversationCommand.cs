using FluentValidation;
using MediatR;
using Philo.Domain.Entities;
using Philo.Domain.Enums;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Conversations.ReopenConversation
{
    /// <summary>A política de reabertura é explícita: só quem tem papel agent/admin pode reabrir (validado no controller/policy).</summary>
    public sealed record ReopenConversationCommand(long OrganizationId, long ConversationId, long ActorId) : IRequest<Result>;

    public sealed class ReopenConversationValidator : AbstractValidator<ReopenConversationCommand>
    {
        public ReopenConversationValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.ActorId).GreaterThan(0);
        }
    }

    public sealed class ReopenConversationHandler : IRequestHandler<ReopenConversationCommand, Result>
    {
        private readonly IConversationRepository _conversations;
        private readonly IConversationEventRepository _events;
        private readonly IUnitOfWork _unitOfWork;

        public ReopenConversationHandler(IConversationRepository conversations, IConversationEventRepository events, IUnitOfWork unitOfWork)
        {
            _conversations = conversations;
            _events = events;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ReopenConversationCommand request, CancellationToken cancellationToken) =>
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var conversation = await _conversations.GetByIdForOrganizationAsync(request.OrganizationId, request.ConversationId, ct);
                if (conversation is null)
                    return Result.Failure(DomainErrors.Conversation.NotFound);

                var result = conversation.Reopen();
                if (result.IsFailure)
                    return result;

                await _conversations.UpdateAsync(conversation, ct);

                var evt = ConversationEvent.Create(
                    request.OrganizationId, request.ConversationId, request.ActorId,
                    ConversationEventType.StatusChanged, "{\"to\":\"open\",\"reopened\":true}");
                await _events.AddAsync(evt, ct);

                return Result.Success();
            }, cancellationToken);
    }
}
