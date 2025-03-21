using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface IGenericService<in T> where T : BaseEntity
    {
        Task AddAsync(T entity);
        Task DeleteAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task UpdateAsync(T entity);
        Task UpdateStatusAsync(int id, bool status);
    }
}
