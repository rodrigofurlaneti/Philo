using Philo.Domain.Primitives;

namespace Philo.Domain.Interfaces
{
    /// <summary>
    /// Contrato mínimo comum aos repositórios de agregados com Id numérico.
    /// Cada operação de escrita persiste imediatamente (SaveChanges), conforme convenção do projeto;
    /// operações que tocam mais de um agregado usam <see cref="IUnitOfWork.ExecuteInTransactionAsync{TResult}"/>.
    /// </summary>
    public interface IBaseRepository<T> where T : AggregateRoot
    {
        Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    }
}
