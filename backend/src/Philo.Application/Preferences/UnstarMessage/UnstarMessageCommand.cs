using FluentValidation;
using MediatR;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Preferences.UnstarMessage
{
    public sealed record UnstarMessageCommand(long OrganizationId, long ConversationId, long MessageId, long UserId) : IRequest<Result>;

    public sealed class UnstarMessageValidator : AbstractValidator<UnstarMessageCommand>
    {
        public UnstarMessageValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.MessageId).GreaterThan(0);
            RuleFor(x => x.UserId).GreaterThan(0);
        }
    }

    public sealed class UnstarMessageHandler : IRequestHandler<UnstarMessageCommand, Result>
    {
        private readonly IMessageUserPreferenceRepository _preferences;

        public UnstarMessageHandler(IMessageUserPreferenceRepository preferences) => _preferences = preferences;

        public async Task<Result> Handle(UnstarMessageCommand request, CancellationToken cancellationToken)
        {
            var preference = await _preferences.GetAsync(request.OrganizationId, request.ConversationId, request.MessageId, request.UserId, cancellationToken);
            if (preference is null)
                return Result.Success();

            preference.Unstar();
            await _preferences.UpdateAsync(preference, cancellationToken);
            return Result.Success();
        }
    }
}
