using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace SalesService.Infrastructure.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task AddAsync(T entity);
        Task<T> AddAsyncReturnObject(T entity);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task DeleteAsync(int id);
        Task DeleteByCompositeKeyAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindIncludingAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includes);
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetByUserIdAsync(int userId);
        Task<T> GetIncludingAsync(int id, params Expression<Func<T, object>>[] includes);
        Task<T> GetIncludingAsync(int id, Func<IQueryable<T>, IQueryable<T>> includeFunc);
        Task RollbackTransactionAsync();
        Task SaveChangesAsync();
        void Update(T entity);
        Task UpdateStatusAsync(int id, bool isActive);
    }
}
