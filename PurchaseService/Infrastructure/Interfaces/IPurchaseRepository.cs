
using PurchaseService.Infrastructure.Models;

namespace PurchaseService.Infrastructure.Interfaces
{
    public interface IPurchaseRepository
    {
        Task<Purchase> AddAsync(Purchase purchase);
        Task<IEnumerable<Purchase>> GetAllAsync();
        Task<IEnumerable<Purchase>> GetAllAsync(int pageNumber, int pageSize);
        Task<Purchase?> GetByIdAsync(int id);
        Task<IEnumerable<Purchase>> GetPendingStockAsync();
        Task<int> GetTotalCountAsync();
        Task SaveChangesAsync();
    }
}
