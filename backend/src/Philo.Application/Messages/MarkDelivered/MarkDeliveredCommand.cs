using FluentValidation;
using MediatR;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Messages.MarkDelivered
{
    /// <summary>Atualiza apenas o recibo do próprio usuário autenticado, com horário do servidor.</summary>
    public sealed record MarkDeliveredCommand(long OrganizationId, long ConversationId, long MessageId, long UserId) : IRequest<Result>;

    public sealed class MarkDeliveredValidator : AbstractValidator<MarkDeliveredCommand>
    {
        public MarkDeliveredValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.MessageId).GreaterThan(0);
            RuleFor(x => x.UserId).GreaterThan(0);
        }
    }

    public sealed class MarkDeliveredHandler : IRequestHandler<MarkDeliveredCommand, Result>
    {
        private readonly IMessageReceiptRepository _receipts;

        public MarkDeliveredHandler(IMessageReceiptRepository receipts) => _receipts = receipts;

        public async Task<Result> Handle(MarkDeliveredCommand request, CancellationToken cancellationToken)
        {
            var receipt = await _receipts.GetAsync(request.OrganizationId, request.ConversationId, request.MessageId, request.UserId, cancellationToken);
            if (receipt is null)
                return Result.Failure(DomainErrors.Receipt.NotFound);

            receipt.MarkDelivered();
            await _receipts.UpdateAsync(receipt, cancellationToken);
            return Result.Success();
        }
    }
}
