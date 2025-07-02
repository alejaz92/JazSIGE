
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseService.Infrastructure.Models;

namespace PurchaseService.Infrastructure.Interfaces
{
    public interface IPurchaseRepository
    {
        Task<Purchase> AddAsync(Purchase purchase);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<IEnumerable<Purchase>> GetAllAsync();
        Task<IEnumerable<Purchase>> GetAllAsync(int pageNumber, int pageSize);
        Task<IEnumerable<Purchase_Article>> GetByArticleIdAsync(int articleId);
        Task<Purchase?> GetByIdAsync(int id);
        Task<IEnumerable<Purchase>> GetPendingStockAsync();
        Task<int> GetTotalCountAsync();
        Task MarkAsDeliveredAsync(int purchaseId);
        Task SaveChangesAsync();
    }
}
