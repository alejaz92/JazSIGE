using AccountingService.Infrastructure.Data;
using AccountingService.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace AccountingService.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AccountingDbContext _context;
        private readonly DbSet<T> _dbSet;
        private IDbContextTransaction? _transaction;

        public GenericRepository(AccountingDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // Lectura
        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsQueryable();
            foreach (var include in includes) query = query.Include(include);
            return await query.ToListAsync();
        }

        public async Task<T?> GetIncludingAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsQueryable();
            foreach (var include in includes) query = query.Include(include);
            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        // Escritura
        public Task AddAsync(T entity) => _dbSet.AddAsync(entity).AsTask();

        public async Task<T> AddAsyncReturnObject(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public void Update(T entity) => _dbSet.Update(entity);

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null) _dbSet.Remove(entity);
        }

        public async Task DeleteByCompositeKeyAsync(Expression<Func<T, bool>> predicate)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(predicate);
            if (entity != null) _dbSet.Remove(entity);
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();

        // Transacciones
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
            return _transaction;
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        // Helpers por convención
        public async Task UpdateStatusAsync(int id, bool isActive)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                var prop = entity.GetType().GetProperty("IsActive");
                if (prop != null && prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(entity, isActive);
                }
            }
        }

        public async Task<IEnumerable<T>> GetByUserIdAsync(int userId)
            => await _context.Set<T>().Where(e => EF.Property<int>(e, "UserId") == userId).ToListAsync();
    }
}
