namespace Philo.Application.Common.Interfaces
{
    public sealed record IssuedToken(string AccessToken, DateTime ExpiresAtUtc);

    public interface IJwtTokenService
    {
        IssuedToken IssueToken(long userId, long organizationId, string role, string sessionId);
    }
}
