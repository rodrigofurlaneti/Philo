using Philo.Domain.Entities;

namespace Philo.Domain.Interfaces
{
    public interface IMessageAttachmentRepository
    {
        Task<MessageAttachment?> GetAsync(long organizationId, long conversationId, long attachmentId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<MessageAttachment>> GetByMessageAsync(long organizationId, long conversationId, long messageId, CancellationToken cancellationToken = default);
        Task AddAsync(MessageAttachment attachment, CancellationToken cancellationToken = default);
    }
}
