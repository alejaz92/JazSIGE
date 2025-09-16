using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace AccountingService.Infrastructure.Interfaces
{
    public interface IGenericRepository<T> where  T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includes);
        Task<T?> GetIncludingAsync(int id, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);
        Task<T> AddAsyncReturnObject(T entity);
        void Update(T entity);
        Task DeleteAsync(int id);
        Task DeleteByCompositeKeyAsync(Expression<Func<T, bool>> predicate);

        Task SaveChangesAsync();

        // Transacciones (igual que en tus otros microservicios)
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        // Opcionales que usan convención por nombre de propiedad
        Task UpdateStatusAsync(int id, bool isActive);
        Task<IEnumerable<T>> GetByUserIdAsync(int userId);
    }
}
