using FluentValidation;
using MediatR;
using Philo.Domain.Entities;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Preferences.StarMessage
{
    /// <summary>Favoritar é uma preferência individual: não afeta os demais participantes.</summary>
    public sealed record StarMessageCommand(long OrganizationId, long ConversationId, long MessageId, long UserId) : IRequest<Result>;

    public sealed class StarMessageValidator : AbstractValidator<StarMessageCommand>
    {
        public StarMessageValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.MessageId).GreaterThan(0);
            RuleFor(x => x.UserId).GreaterThan(0);
        }
    }

    public sealed class StarMessageHandler : IRequestHandler<StarMessageCommand, Result>
    {
        private readonly IMessageUserPreferenceRepository _preferences;

        public StarMessageHandler(IMessageUserPreferenceRepository preferences) => _preferences = preferences;

        public async Task<Result> Handle(StarMessageCommand request, CancellationToken cancellationToken)
        {
            var preference = await _preferences.GetAsync(request.OrganizationId, request.ConversationId, request.MessageId, request.UserId, cancellationToken);
            if (preference is null)
            {
                preference = MessageUserPreference.Create(request.OrganizationId, request.ConversationId, request.MessageId, request.UserId);
                preference.Star();
                await _preferences.AddAsync(preference, cancellationToken);
            }
            else
            {
                preference.Star();
                await _preferences.UpdateAsync(preference, cancellationToken);
            }

            return Result.Success();
        }
    }
}
