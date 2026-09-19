using FluentValidation;
using MediatR;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Preferences.UnhideMessage
{
    public sealed record UnhideMessageCommand(long OrganizationId, long ConversationId, long MessageId, long UserId) : IRequest<Result>;

    public sealed class UnhideMessageValidator : AbstractValidator<UnhideMessageCommand>
    {
        public UnhideMessageValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.MessageId).GreaterThan(0);
            RuleFor(x => x.UserId).GreaterThan(0);
        }
    }

    public sealed class UnhideMessageHandler : IRequestHandler<UnhideMessageCommand, Result>
    {
        private readonly IMessageUserPreferenceRepository _preferences;

        public UnhideMessageHandler(IMessageUserPreferenceRepository preferences) => _preferences = preferences;

        public async Task<Result> Handle(UnhideMessageCommand request, CancellationToken cancellationToken)
        {
            var preference = await _preferences.GetAsync(request.OrganizationId, request.ConversationId, request.MessageId, request.UserId, cancellationToken);
            if (preference is null)
                return Result.Success();

            preference.Unhide();
            await _preferences.UpdateAsync(preference, cancellationToken);
            return Result.Success();
        }
    }
}
