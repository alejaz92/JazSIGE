using PurchaseService.Infrastructure.Models;

namespace PurchaseService.Infrastructure.Interfaces
{
    public interface IPurchaseDocumentRepository
    {
        Task AddAsync(PurchaseDocument entity);
        Task DeleteAsync(PurchaseDocument entity);
        Task<bool> ExistsByNumberAsync(int purchaseId, PurchaseDocumentType type, string number, bool onlyActive = false);
        Task<bool> ExistsInvoiceAsync(int purchaseId, bool onlyActive = false);
        Task<PurchaseDocument?> GetByIdAsync(int id);
        Task<IReadOnlyList<PurchaseDocument>> GetByPurchaseIdAsync(int purchaseId, bool onlyActive = false);
        Task<PurchaseDocument?> GetInvoiceByPurchaseIdAsync(int purchaseId, bool onlyActive = false);
        Task UpdateAsync(PurchaseDocument entity);
    }
}
