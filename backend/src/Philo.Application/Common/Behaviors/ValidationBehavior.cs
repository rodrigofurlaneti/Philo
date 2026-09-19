using FluentValidation;
using MediatR;
using Philo.Domain.Primitives;

namespace Philo.Application.Common.Behaviors
{
    /// <summary>
    /// Executa todos os validadores FluentValidation registrados para a requisição antes do handler.
    /// Cada validador roda em seu próprio ValidationContext (sem estado compartilhado entre execuções paralelas).
    /// Só se aplica a requisições que retornam Result/Result&lt;T&gt;; falhas viram Result.Failure com um erro de validação agregando os detalhes.
    /// </summary>
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
                .SelectMany(result => result.Errors)
                .Where(failure => failure is not null)
                .ToList();

            if (failures.Count == 0)
                return await next();

            var errors = failures
                .Select(f => Error.Validation(f.ErrorCode ?? f.PropertyName, f.ErrorMessage))
                .ToArray();

            var validationError = new ValidationError(errors);

            if (typeof(TResponse) == typeof(Result))
                return (TResponse)(object)Result.Failure(validationError);

            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = typeof(Result)
                    .GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
                    .MakeGenericMethod(typeof(TResponse).GetGenericArguments()[0]);

                return (TResponse)failureMethod.Invoke(null, [validationError])!;
            }

            throw new ValidationException(failures);
        }
    }
}
