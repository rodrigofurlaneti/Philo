using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Philo.WebApi.Auth
{
    /// <summary>
    /// Protege endpoints internos (ex.: emissão de sessão a partir da autenticação já feita
    /// pelo site) com uma credencial de serviço, nunca aceitando papel/empresa vindos do chamador.
    /// </summary>
    public sealed class InternalApiKeyFilter : IAsyncActionFilter
    {
        private const string HeaderName = "X-Internal-Api-Key";

        private readonly InternalApiKeyOptions _options;

        public InternalApiKeyFilter(IOptions<InternalApiKeyOptions> options) => _options = options.Value;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (string.IsNullOrEmpty(_options.ApiKey) ||
                !context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var provided) ||
                provided.Count == 0 ||
                !CryptographicEquals(provided[0]!, _options.ApiKey))
            {
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }

            await next();
        }

        private static bool CryptographicEquals(string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(a), System.Text.Encoding.UTF8.GetBytes(b));
        }
    }
}
