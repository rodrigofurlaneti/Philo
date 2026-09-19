using Philo.Domain.Enums;
using Philo.Domain.Primitives;

namespace Philo.Domain.Entities
{
    public sealed class Conversation : AggregateRoot
    {
        public long OrganizationId { get; private set; }
        public long CustomerId { get; private set; }
        public long? AssignedTo { get; private set; }
        public long CreatedBy { get; private set; }
        public ConversationPurpose Purpose { get; private set; }
        public string? Subject { get; private set; }
        public ConversationStatus Status { get; private set; }
        public Priority Priority { get; private set; }
        public string? SourcePageUrl { get; private set; }
        public DateTime LastActivityAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? ClosedAt { get; private set; }

        private readonly List<ConversationParticipant> _participants = [];
        public IReadOnlyCollection<ConversationParticipant> Participants => _participants.AsReadOnly();

        private readonly List<ConversationProduct> _products = [];
        public IReadOnlyCollection<ConversationProduct> Products => _products.AsReadOnly();

        private Conversation() : base(0) { }

        private Conversation(
            long organizationId, long customerId, long createdBy,
            ConversationPurpose purpose, string? subject, Priority priority, string? sourcePageUrl)
            : base(0)
        {
            OrganizationId = organizationId;
            CustomerId = customerId;
            CreatedBy = createdBy;
            Purpose = purpose;
            Subject = subject;
            Priority = priority;
            SourcePageUrl = sourcePageUrl;
            Status = ConversationStatus.Open;
            var now = DateTime.UtcNow;
            LastActivityAt = now;
            CreatedAt = now;
            UpdatedAt = now;
        }

        /// <summary>Cria a conversa (ainda sem Id, atribuído pelo banco ao persistir).</summary>
        public static Result<Conversation> Open(
            long organizationId, long customerId, long createdBy,
            ConversationPurpose purpose, string? subject, Priority priority, string? sourcePageUrl)
        {
            if (organizationId <= 0 || customerId <= 0 || createdBy <= 0)
                return Result.Failure<Conversation>(Error.Validation("Conversation.InvalidIds", "Organização, cliente e criador são obrigatórios."));

            return Result.Success(new Conversation(organizationId, customerId, createdBy, purpose, subject, priority, sourcePageUrl));
        }

        /// <summary>
        /// Registra o cliente como o primeiro participante ativo. Deve ser chamado depois que a
        /// conversa já foi persistida (Id gerado pelo banco) — o README exige que toda conversa
        /// comece com o cliente como participante.
        /// </summary>
        public Result<ConversationParticipant> AddInitialCustomerParticipant()
        {
            var participantResult = ConversationParticipant.Create(OrganizationId, Id, CustomerId);
            if (participantResult.IsFailure)
                return participantResult;

            _participants.Add(participantResult.Value);
            return participantResult;
        }

        public Result AssignTo(long agentId)
        {
            if (Status == ConversationStatus.Closed)
                return Result.Failure(Domain.Errors.DomainErrors.Conversation.AlreadyClosed);

            AssignedTo = agentId;
            if (Status == ConversationStatus.Open)
                Status = ConversationStatus.WaitingCustomer;
            Touch();
            return Result.Success();
        }

        public Result ChangeStatus(ConversationStatus status)
        {
            if (Status == ConversationStatus.Closed && status != ConversationStatus.Closed)
                return Result.Failure(Domain.Errors.DomainErrors.Conversation.AlreadyClosed);

            Status = status;
            Touch();
            return Result.Success();
        }

        public Result Close()
        {
            if (Status == ConversationStatus.Closed)
                return Result.Failure(Domain.Errors.DomainErrors.Conversation.AlreadyClosed);

            Status = ConversationStatus.Closed;
            ClosedAt = DateTime.UtcNow;
            UpdatedAt = ClosedAt.Value;
            return Result.Success();
        }

        public Result Reopen()
        {
            if (Status != ConversationStatus.Closed)
                return Result.Failure(Domain.Errors.DomainErrors.Conversation.NotClosed);

            Status = ConversationStatus.Open;
            ClosedAt = null;
            Touch();
            return Result.Success();
        }

        public void RegisterActivity(DateTime at)
        {
            if (at > LastActivityAt)
                LastActivityAt = at;
        }

        private void Touch() => UpdatedAt = DateTime.UtcNow;

        public bool IsClosed => Status == ConversationStatus.Closed;
    }
}
