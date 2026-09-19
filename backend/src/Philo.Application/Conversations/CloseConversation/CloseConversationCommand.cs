using FluentValidation;
using MediatR;
using Philo.Domain.Entities;
using Philo.Domain.Enums;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Conversations.CloseConversation
{
    public sealed record CloseConversationCommand(long OrganizationId, long ConversationId, long ActorId) : IRequest<Result>;

    public sealed class CloseConversationValidator : AbstractValidator<CloseConversationCommand>
    {
        public CloseConversationValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.ActorId).GreaterThan(0);
        }
    }

    public sealed class CloseConversationHandler : IRequestHandler<CloseConversationCommand, Result>
    {
        private readonly IConversationRepository _conversations;
        private readonly IConversationEventRepository _events;
        private readonly IUnitOfWork _unitOfWork;

        public CloseConversationHandler(IConversationRepository conversations, IConversationEventRepository events, IUnitOfWork unitOfWork)
        {
            _conversations = conversations;
            _events = events;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(CloseConversationCommand request, CancellationToken cancellationToken) =>
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var conversation = await _conversations.GetByIdForOrganizationAsync(request.OrganizationId, request.ConversationId, ct);
                if (conversation is null)
                    return Result.Failure(DomainErrors.Conversation.NotFound);

                var result = conversation.Close();
                if (result.IsFailure)
                    return result;

                await _conversations.UpdateAsync(conversation, ct);

                var evt = ConversationEvent.Create(
                    request.OrganizationId, request.ConversationId, request.ActorId,
                    ConversationEventType.StatusChanged, "{\"to\":\"closed\"}");
                await _events.AddAsync(evt, ct);

                return Result.Success();
            }, cancellationToken);
    }
}
