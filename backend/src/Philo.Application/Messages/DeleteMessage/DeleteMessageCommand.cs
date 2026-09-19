using FluentValidation;
using MediatR;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Messages.DeleteMessage
{
    /// <summary>Apaga para todos: o corpo é limpo e a mensagem vira um tombstone (README/schema CHECK).</summary>
    public sealed record DeleteMessageCommand(long OrganizationId, long ConversationId, long MessageId, long RequesterId) : IRequest<Result>;

    public sealed class DeleteMessageValidator : AbstractValidator<DeleteMessageCommand>
    {
        public DeleteMessageValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.MessageId).GreaterThan(0);
            RuleFor(x => x.RequesterId).GreaterThan(0);
        }
    }

    public sealed class DeleteMessageHandler : IRequestHandler<DeleteMessageCommand, Result>
    {
        private readonly IMessageRepository _messages;

        public DeleteMessageHandler(IMessageRepository messages) => _messages = messages;

        public async Task<Result> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
        {
            var message = await _messages.GetByIdAsync(request.OrganizationId, request.ConversationId, request.MessageId, cancellationToken);
            if (message is null)
                return Result.Failure(DomainErrors.Message.NotFound);

            var result = message.Delete(request.RequesterId);
            if (result.IsFailure)
                return result;

            await _messages.UpdateAsync(message, cancellationToken);
            return Result.Success();
        }
    }
}
