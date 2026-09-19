namespace Philo.Application.Auth.Common
{
    public sealed record SessionResponse(string AccessToken, DateTime ExpiresAtUtc, long UserId, long OrganizationId, string Role);
}
