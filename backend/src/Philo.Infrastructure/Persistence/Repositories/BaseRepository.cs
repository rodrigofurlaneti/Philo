using Microsoft.EntityFrameworkCore;
using Philo.Domain.Primitives;

namespace Philo.Infrastructure.Persistence.Repositories
{
    /// <summary>Implementação comum a repositórios de agregados com Id numérico. Cada escrita salva imediatamente.</summary>
    public abstract class BaseRepository<T> where T : AggregateRoot
    {
        protected readonly PhiloDbContext Context;

        protected BaseRepository(PhiloDbContext context) => Context = context;

        protected DbSet<T> Set => Context.Set<T>();

        public virtual async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
            await Set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await Set.ToListAsync(cancellationToken);

        public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await Set.AddAsync(entity, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            Set.Update(entity);
            await Context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default) =>
            await Set.AnyAsync(e => e.Id == id, cancellationToken);
    }
}
