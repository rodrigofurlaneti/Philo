namespace Philo.Application.Common.Interfaces
{
    /// <summary>
    /// Identidade extraída do token JWT autenticado. Nunca deve ser preenchida a partir
    /// de dados enviados pelo cliente (body/query) — apenas do contexto autenticado.
    /// </summary>
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }
        long UserId { get; }
        long OrganizationId { get; }
        string Role { get; }
        string SessionId { get; }
    }
}
