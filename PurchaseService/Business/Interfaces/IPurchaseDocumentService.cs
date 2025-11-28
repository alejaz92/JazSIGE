using PurchaseService.Business.Models;
using PurchaseService.Business.Models.Clients;

namespace PurchaseService.Business.Interfaces
{
    public interface IPurchaseDocumentService
    {
        Task CancelAsync(int purchaseId, int documentId, PurchaseDocumentCancelDTO dto, int currentUserId);
        Task CoverInvoiceWithReceiptsAsync(int purchaseId, CoverInvoiceRequest request, CancellationToken ct = default);
        Task<PurchaseDocumentDTO> CreateAsync(int purchaseId, PurchaseDocumentCreateDTO dto, int currentUserId);
        Task<IReadOnlyList<PurchaseDocumentDTO>> GetByPurchaseIdAsync(int purchaseId, bool onlyActive = false);
    }
}
