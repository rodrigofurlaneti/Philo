using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Philo.WebApi.Middleware
{
    /// <summary>
    /// Último recurso para exceções não tratadas. Falhas esperadas de negócio já viram
    /// Result.Failure e nunca chegam aqui; isto captura apenas o inesperado, sem vazar detalhes
    /// internos na resposta (AGENTS.md).
    /// </summary>
    public sealed class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exceção não tratada em {Path}", context.Request.Path);

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var problem = new ProblemDetails
                {
                    Title = "Erro interno.",
                    Detail = "Ocorreu um erro inesperado ao processar a requisição.",
                    Status = (int)HttpStatusCode.InternalServerError,
                };

                await context.Response.WriteAsJsonAsync(problem);
            }
        }
    }
}
