namespace Philo.Application.Messages.Common
{
    /// <summary>Mensagem apagada vira um marcador sem corpo nem anexos (README).</summary>
    public sealed record MessageDto(
        long Id, long SenderId, string MessageType, string? Body,
        long? ReplyToId, DateTime SentAt, DateTime? EditedAt, bool IsDeleted, bool IsStarred);

    public sealed record AttachmentInput(string StorageKey, string OriginalName, string MimeType);
}
