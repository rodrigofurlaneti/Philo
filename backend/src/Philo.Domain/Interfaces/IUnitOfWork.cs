namespace Philo.Domain.Interfaces
{
    /// <summary>
    /// Delimita a transação de uma operação de escrita. Os repositórios apenas
    /// rastreiam mudanças; é o UnitOfWork quem persiste (evita SaveChanges redundante).
    /// </summary>
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>Executa a operação dentro de uma transação explícita (necessária quando
        /// mais de um agregado é alterado na mesma operação de negócio).</summary>
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default);
    }
}
