
using PurchaseService.Infrastructure.Models;

namespace PurchaseService.Infrastructure.Interfaces
{
    public interface IDispatchRepository
    {
        Task<Dispatch> AddAsync(Dispatch dispatch);
        Task<IEnumerable<Dispatch>> GetAllAsync(int pageNumber, int pageSize);
        Task<IEnumerable<Dispatch>> GetAllAsync();
        Task<Dispatch?> GetByIdAsync(int id);
        Task<int> GetTotalCountAsync();
        Task SaveChangesAsync();
    }
}
