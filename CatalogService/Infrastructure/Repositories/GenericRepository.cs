using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace CatalogService.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly CatalogDbContext _context;
        private readonly DbSet<T> _dbSet;
        private IDbContextTransaction _transaction;
        private bool _isTransactionActive = false;

        public GenericRepository(CatalogDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public async Task<T> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
        public async Task<IEnumerable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }
        public async Task<T> GetIncludingAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.FirstOrDefaultAsync(entity => EF.Property<int>(entity, "Id") == id);
        }
        //public async Task<IEnumerable<T>> GetPaginatedAsync(int page, int pageSize, params Expression<Func<T, object>>[] includes)
        //{
        //    IQueryable<T> query = _dbSet;

        //    foreach (var include in includes)
        //    {
        //        query = query.Include(include);
        //    }

        //    return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        //}
        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public async Task<T> AddAsyncReturnObject(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
        public void Update(T entity) => _dbSet.Update(entity);
        public async Task UpdateStatusAsync(int id, bool isActive)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                var property = entity.GetType().GetProperty("IsActive");
                property.SetValue(entity, isActive);
            }
        }
        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }
        public async Task DeleteByCompositeKeyAsync(Expression<Func<T, bool>> predicate)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(predicate);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => await _dbSet.Where(predicate).ToListAsync();
        public async Task<IEnumerable<T>> GetByUserIdAsync(int userId)
        => await _context.Set<T>().Where(entity => EF.Property<int>(entity, "UserId") == userId).ToListAsync();
        public async Task<IDbContextTransaction> BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
                _isTransactionActive = false;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
                _isTransactionActive = false;
            }
        }

    }
}
