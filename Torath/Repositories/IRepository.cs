using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Torath.Repositories
{
    // The <T> means this can accept ANY of your database entities (Book, Article, etc.)
    public interface IRepository<T> where T : class
    {
        // Notice the CancellationToken added to every async method!
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        // We expose an IQueryable so services can still do custom filtering & pagination
        IQueryable<T> GetQueryable();

        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        void Update(T entity); // Update in EF is usually sync
        void Delete(T entity); // Delete in EF is usually sync

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}