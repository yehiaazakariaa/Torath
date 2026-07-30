using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Torath.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly TorathDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(TorathDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            // The cancellation token tells SQL to abort if the user cancels the request
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public async Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            // FindAsync doesn't accept a token easily with params, so we use SingleOrDefaultAsync
            var keyProperty = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties[0];
            return await _dbSet.SingleOrDefaultAsync(e => EF.Property<int>(e, keyProperty.Name) == id, cancellationToken);
        }

        public IQueryable<T> GetQueryable()
        {
            // Allows Services to apply custom .Where() or .Skip() logic before hitting the DB
            return _dbSet.AsQueryable();
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}