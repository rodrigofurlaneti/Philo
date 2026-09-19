using FluentValidation;
using MediatR;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Messages.MarkRead
{
    public sealed record MarkReadCommand(long OrganizationId, long ConversationId, long MessageId, long UserId) : IRequest<Result>;

    public sealed class MarkReadValidator : AbstractValidator<MarkReadCommand>
    {
        public MarkReadValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.MessageId).GreaterThan(0);
            RuleFor(x => x.UserId).GreaterThan(0);
        }
    }

    public sealed class MarkReadHandler : IRequestHandler<MarkReadCommand, Result>
    {
        private readonly IMessageReceiptRepository _receipts;

        public MarkReadHandler(IMessageReceiptRepository receipts) => _receipts = receipts;

        public async Task<Result> Handle(MarkReadCommand request, CancellationToken cancellationToken)
        {
            var receipt = await _receipts.GetAsync(request.OrganizationId, request.ConversationId, request.MessageId, request.UserId, cancellationToken);
            if (receipt is null)
                return Result.Failure(DomainErrors.Receipt.NotFound);

            var result = receipt.MarkRead();
            if (result.IsFailure)
                return result;

            await _receipts.UpdateAsync(receipt, cancellationToken);
            return Result.Success();
        }
    }
}
