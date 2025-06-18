using StockService.Infrastructure.Models;

namespace StockService.Infrastructure.Interfaces
{
    public interface IPendingStockEntryRepository
    {
        Task AddAsync(PendingStockEntry entry);
        Task<List<PendingStockEntry>> GetByPurchaseIdAsync(int purchaseId);
        Task MarkAsProcessedAsync(int id);
    }
}
