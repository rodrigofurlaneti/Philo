namespace Philo.Application.Conversations.Common
{
    public sealed record ConversationSummaryDto(
        long Id, long CustomerId, long? AssignedTo, string Purpose, string? Subject,
        string Status, string Priority, DateTime LastActivityAt);

    public sealed record ConversationParticipantDto(long UserId, DateTime JoinedAt, DateTime? LeftAt, bool IsActive);

    public sealed record ConversationDetailsDto(
        long Id, long OrganizationId, long CustomerId, long? AssignedTo, long CreatedBy,
        string Purpose, string? Subject, string Status, string Priority, string? SourcePageUrl,
        DateTime LastActivityAt, DateTime CreatedAt, DateTime? ClosedAt,
        IReadOnlyList<ConversationParticipantDto> Participants);
}
