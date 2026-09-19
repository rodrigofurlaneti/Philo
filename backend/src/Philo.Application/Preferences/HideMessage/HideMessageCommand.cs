using FluentValidation;
using MediatR;
using Philo.Domain.Entities;
using Philo.Domain.Interfaces;
using Philo.Domain.Primitives;

namespace Philo.Application.Preferences.HideMessage
{
    /// <summary>Ocultar é individual: 'apagar apenas para mim'. Não altera a mensagem para os demais.</summary>
    public sealed record HideMessageCommand(long OrganizationId, long ConversationId, long MessageId, long UserId) : IRequest<Result>;

    public sealed class HideMessageValidator : AbstractValidator<HideMessageCommand>
    {
        public HideMessageValidator()
        {
            RuleFor(x => x.OrganizationId).GreaterThan(0);
            RuleFor(x => x.ConversationId).GreaterThan(0);
            RuleFor(x => x.MessageId).GreaterThan(0);
            RuleFor(x => x.UserId).GreaterThan(0);
        }
    }

    public sealed class HideMessageHandler : IRequestHandler<HideMessageCommand, Result>
    {
        private readonly IMessageUserPreferenceRepository _preferences;

        public HideMessageHandler(IMessageUserPreferenceRepository preferences) => _preferences = preferences;

        public async Task<Result> Handle(HideMessageCommand request, CancellationToken cancellationToken)
        {
            var preference = await _preferences.GetAsync(request.OrganizationId, request.ConversationId, request.MessageId, request.UserId, cancellationToken);
            if (preference is null)
            {
                preference = MessageUserPreference.Create(request.OrganizationId, request.ConversationId, request.MessageId, request.UserId);
                preference.Hide();
                await _preferences.AddAsync(preference, cancellationToken);
            }
            else
            {
                preference.Hide();
                await _preferences.UpdateAsync(preference, cancellationToken);
            }

            return Result.Success();
        }
    }
}
