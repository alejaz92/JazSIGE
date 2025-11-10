using PurchaseService.Business.Models;

namespace PurchaseService.Business.Interfaces
{
    public interface IPurchaseDocumentService
    {
        Task CancelAsync(int purchaseId, int documentId, PurchaseDocumentCancelDTO dto, int currentUserId);
        Task<PurchaseDocumentDTO> CreateAsync(int purchaseId, PurchaseDocumentCreateDTO dto, int currentUserId);
        Task<IReadOnlyList<PurchaseDocumentDTO>> GetByPurchaseIdAsync(int purchaseId, bool onlyActive = false);
    }
}
