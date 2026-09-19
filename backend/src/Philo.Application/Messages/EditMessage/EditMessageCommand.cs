using FluentValidation;
using MediatR;
using Philo.Domain.Errors;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Messages.EditMessage
{
    /// <summary>Apenas o autor edita; não é possível alterar empresa, conversa, autor ou data original.</summary>
    public sealed record EditMessageCommand(long OrganizationId, long ConversationId, long MessageId, long EditorId, string NewBody) : IRequest<Result>;

    public sealed class EditMessageValidator : AbstractValidator<EditMessageCommand>
    {
        public EditMessageValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.MessageId).GreaterThan(0);
            RuleFor(x => x.EditorId).GreaterThan(0);
            RuleFor(x => x.NewBody).NotEmpty().MaximumLength(8000);
        }
    }

    public sealed class EditMessageHandler : IRequestHandler<EditMessageCommand, Result>
    {
        private readonly IMessageRepository _messages;

        public EditMessageHandler(IMessageRepository messages) => _messages = messages;

        public async Task<Result> Handle(EditMessageCommand request, CancellationToken cancellationToken)
        {
            var message = await _messages.GetByIdAsync(request.OrganizationId, request.ConversationId, request.MessageId, cancellationToken);
            if (message is null)
                return Result.Failure(DomainErrors.Message.NotFound);

            var result = message.Edit(request.EditorId, request.NewBody);
            if (result.IsFailure)
                return result;

            await _messages.UpdateAsync(message, cancellationToken);
            return Result.Success();
        }
    }
}
