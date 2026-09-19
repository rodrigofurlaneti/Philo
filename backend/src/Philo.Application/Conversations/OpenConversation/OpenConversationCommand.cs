using FluentValidation;
using MediatR;
using Philo.Domain.Entities;
using Philo.Domain.Enums;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Conversations.OpenConversation
{
    /// <summary>
    /// Abre um novo atendimento. O cliente informado precisa ter papel 'customer' na organização;
    /// um mesmo cliente pode ter várias conversas simultâneas (não há limite artificial).
    /// </summary>
    public sealed record OpenConversationCommand(
        long OrganizationId, long CustomerId, long CreatedBy,
        ConversationPurpose Purpose, string? Subject, Priority Priority, string? SourcePageUrl) : IRequest<Result<long>>;

    public sealed class OpenConversationValidator : AbstractValidator<OpenConversationCommand>
    {
        public OpenConversationValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.CustomerId).GreaterThan(0);
            RuleFor(x => x.CreatedBy).GreaterThan(0);
            RuleFor(x => x.Subject).MaximumLength(200);
            RuleFor(x => x.SourcePageUrl).MaximumLength(2048);
        }
    }

    public sealed class OpenConversationHandler : IRequestHandler<OpenConversationCommand, Result<long>>
    {
        private readonly IConversationRepository _conversations;
        private readonly IConversationParticipantRepository _participants;
        private readonly IOrganizationUserRepository _memberships;
        private readonly IConversationEventRepository _events;
        private readonly IUnitOfWork _unitOfWork;

        public OpenConversationHandler(
            IConversationRepository conversations, IConversationParticipantRepository participants,
            IOrganizationUserRepository memberships, IConversationEventRepository events, IUnitOfWork unitOfWork)
        {
            _conversations = conversations;
            _participants = participants;
            _memberships = memberships;
            _events = events;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<long>> Handle(OpenConversationCommand request, CancellationToken cancellationToken)
        {
            var customerMembership = await _memberships.GetAsync(request.OrganizationId, request.CustomerId, cancellationToken);
            if (customerMembership is null || !customerMembership.IsActive)
                return Result.Failure<long>(DomainErrors.Membership.NotFound);
            if (customerMembership.Role != Role.Customer)
                return Result.Failure<long>(DomainErrors.Conversation.CustomerMustBeCustomerRole);

            var creatorMembership = await _memberships.GetAsync(request.OrganizationId, request.CreatedBy, cancellationToken);
            if (creatorMembership is null || !creatorMembership.IsActive)
                return Result.Failure<long>(DomainErrors.Membership.NotFound);

            return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var conversationResult = Conversation.Open(
                    request.OrganizationId, request.CustomerId, request.CreatedBy,
                    request.Purpose, request.Subject, request.Priority, request.SourcePageUrl);

                if (conversationResult.IsFailure)
                    return Result.Failure<long>(conversationResult.Error);

                var conversation = conversationResult.Value;
                await _conversations.AddAsync(conversation, ct);

                var participantResult = conversation.AddInitialCustomerParticipant();
                if (participantResult.IsFailure)
                    return Result.Failure<long>(participantResult.Error);

                await _participants.AddAsync(participantResult.Value, ct);

                var evt = ConversationEvent.Create(
                    conversation.OrganizationId, conversation.Id, request.CreatedBy,
                    ConversationEventType.Created, $"{{\"purpose\":\"{conversation.Purpose}\"}}");
                await _events.AddAsync(evt, ct);

                return Result.Success(conversation.Id);
            }, cancellationToken);
        }
    }
}
