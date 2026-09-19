namespace Philo.Domain.Primitives
{
    public record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
        public static readonly Error NullValue = new("Error.NullValue", "O valor fornecido é nulo.", ErrorType.Validation);
        public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
        public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
        public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
        public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
        public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
        public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);
    }
    public sealed record ValidationError(Error[] Errors)
        : Error("Validation.General", "Uma ou mais falhas de validação ocorreram.", ErrorType.Validation);
}
