using FluentValidation;
using MediatR;
using Philo.Domain.Entities;
using Philo.Domain.Enums;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Conversations.ChangeConversationStatus
{
    public sealed record ChangeConversationStatusCommand(long OrganizationId, long ConversationId, ConversationStatus Status, long ActorId) : IRequest<Result>;

    public sealed class ChangeConversationStatusValidator : AbstractValidator<ChangeConversationStatusCommand>
    {
        public ChangeConversationStatusValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.ActorId).GreaterThan(0);
            RuleFor(x => x.Status).IsInEnum().NotEqual(ConversationStatus.Closed)
                .WithMessage("Use o endpoint de encerramento para fechar a conversa.");
        }
    }

    public sealed class ChangeConversationStatusHandler : IRequestHandler<ChangeConversationStatusCommand, Result>
    {
        private readonly IConversationRepository _conversations;
        private readonly IConversationEventRepository _events;
        private readonly IUnitOfWork _unitOfWork;

        public ChangeConversationStatusHandler(IConversationRepository conversations, IConversationEventRepository events, IUnitOfWork unitOfWork)
        {
            _conversations = conversations;
            _events = events;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ChangeConversationStatusCommand request, CancellationToken cancellationToken) =>
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var conversation = await _conversations.GetByIdForOrganizationAsync(request.OrganizationId, request.ConversationId, ct);
                if (conversation is null)
                    return Result.Failure(DomainErrors.Conversation.NotFound);

                var previousStatus = conversation.Status;
                var result = conversation.ChangeStatus(request.Status);
                if (result.IsFailure)
                    return result;

                await _conversations.UpdateAsync(conversation, ct);

                var evt = ConversationEvent.Create(
                    request.OrganizationId, request.ConversationId, request.ActorId,
                    ConversationEventType.StatusChanged,
                    $"{{\"from\":\"{previousStatus}\",\"to\":\"{request.Status}\"}}");
                await _events.AddAsync(evt, ct);

                return Result.Success();
            }, cancellationToken);
    }
}
