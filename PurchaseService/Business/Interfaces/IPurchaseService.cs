using PurchaseService.Business.Models;

namespace PurchaseService.Business.Interfaces
{
    public interface IPurchaseService
    {
        Task<PurchaseDocumentDTO?> CreateAsync(PurchaseCreateDTO dto, int userId);
        Task<IEnumerable<PurchaseDTO>> GetAllAsync();
        Task<(IEnumerable<PurchaseDTO> purchases, int totalCount)> GetAllAsync(int pageNumber, int pageSize);
        Task<PurchaseDTO?> GetByIdAsync(int id);
        Task<IEnumerable<PurchaseDTO>> GetPendingStockAsync();
        Task<IEnumerable<ArticlePurchaseHistoryDTO>> GetPurchaseHistoryByArticleIdAsync(int articleId);
        Task RegisterStockFromPendingAsync(int purchaseId, int warehouseId, string reference, int userId, int? dispatchId);
        Task<StockApplyAdjustmentResultDTO?> UpdateArticlesAsync(int purchaseId, IEnumerable<PurchaseArticleUpdateDTO> updates, int userId);
        //Task<int> RetryAllPendingStockAsync(int userId);
        //Task RetryStockUpdateAsync(int purchaseId, int userId);
    }
}
