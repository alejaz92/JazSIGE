using PurchaseService.Business.Models;

namespace PurchaseService.Business.Interfaces
{
    public interface IPurchaseService
    {
        Task<int> CreateAsync(PurchaseCreateDTO dto, int userId);
        Task<IEnumerable<PurchaseDTO>> GetAllAsync();
        Task<(IEnumerable<PurchaseDTO> purchases, int totalCount)> GetAllAsync(int pageNumber, int pageSize);
        Task<PurchaseDTO?> GetByIdAsync(int id);
        Task<IEnumerable<PurchaseDTO>> GetPendingStockAsync();
        Task<int> RetryAllPendingStockAsync(int userId);
        Task RetryStockUpdateAsync(int purchaseId, int userId);
    }
}
