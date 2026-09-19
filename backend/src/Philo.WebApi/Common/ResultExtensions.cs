using Microsoft.AspNetCore.Mvc;
using Philo.Domain.Primitives;

namespace Philo.WebApi.Common
{
    /// <summary>Traduz Result/Result&lt;T&gt; do domínio para respostas HTTP, preservando o ErrorType.</summary>
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult(this Result result, ControllerBase controller) =>
            result.IsSuccess ? controller.NoContent() : Problem(result.Error, controller);

        public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller, Func<T, IActionResult>? onSuccess = null)
        {
            if (result.IsFailure)
                return Problem(result.Error, controller);

            return onSuccess is not null ? onSuccess(result.Value) : controller.Ok(result.Value);
        }

        private static IActionResult Problem(Error error, ControllerBase controller)
        {
            if (error is ValidationError validationError)
            {
                foreach (var e in validationError.Errors)
                    controller.ModelState.AddModelError(e.Code, e.Message);

                return controller.ValidationProblem(controller.ModelState);
            }

            var statusCode = error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError,
            };

            return controller.Problem(title: error.Code, detail: error.Message, statusCode: statusCode);
        }
    }
}
