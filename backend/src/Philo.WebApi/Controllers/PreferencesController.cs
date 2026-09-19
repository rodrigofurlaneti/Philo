using MediatR;
using Microsoft.AspNetCore.Mvc;
using Philo.Application.Common.Interfaces;
using Philo.Application.Preferences.HideMessage;
using Philo.Application.Preferences.StarMessage;
using Philo.Application.Preferences.UnhideMessage;
using Philo.Application.Preferences.UnstarMessage;
using Philo.WebApi.Common;

namespace Philo.WebApi.Controllers
{
    /// <summary>Favoritar/ocultar são preferências individuais (README): não afetam outros participantes.</summary>
    [Route("api/organizations/{organizationId:long}/conversations/{conversationId:long}/messages/{messageId:long}/preferences")]
    public sealed class PreferencesController : PhiloControllerBase
    {
        public PreferencesController(ISender mediator, ICurrentUserService currentUser) : base(mediator, currentUser) { }

        [HttpPut("star")]
        public async Task<IActionResult> Star(long organizationId, long conversationId, long messageId, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;
            var result = await Mediator.Send(new StarMessageCommand(organizationId, conversationId, messageId, CurrentUser.UserId), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpDelete("star")]
        public async Task<IActionResult> Unstar(long organizationId, long conversationId, long messageId, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;
            var result = await Mediator.Send(new UnstarMessageCommand(organizationId, conversationId, messageId, CurrentUser.UserId), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPut("hide")]
        public async Task<IActionResult> Hide(long organizationId, long conversationId, long messageId, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;
            var result = await Mediator.Send(new HideMessageCommand(organizationId, conversationId, messageId, CurrentUser.UserId), cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpDelete("hide")]
        public async Task<IActionResult> Unhide(long organizationId, long conversationId, long messageId, CancellationToken cancellationToken)
        {
            if (EnsureOrganization(organizationId) is { } forbidden) return forbidden;
            var result = await Mediator.Send(new UnhideMessageCommand(organizationId, conversationId, messageId, CurrentUser.UserId), cancellationToken);
            return result.ToActionResult(this);
        }
    }
}
