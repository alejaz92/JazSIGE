using SalesService.Business.Models.Clients;
using SalesService.Business.Models.Sale;
using SalesService.Business.Models.Sale.fiscalDocs;

namespace SalesService.Business.Interfaces
{
    public interface ISaleService
    {
        Task CoverInvoiceWithReceiptsAsync(int saleId, CoverInvoiceRequest request, CancellationToken ct = default);
        Task<SaleDetailDTO> CreateAsync(SaleCreateDTO dto);
        Task<InvoiceBasicDTO> CreateCreditNoteAsync(int saleId, CreditNoteCreateForSaleDTO dto);
        Task<InvoiceBasicDTO> CreateDebitNoteAsync(int saleId, DebitNoteCreateForSaleDTO dto);
        Task<InvoiceBasicDTO> CreateInvoiceAsync(int saleId);
        Task<QuickSaleResultDTO> CreateQuickAsync(QuickSaleCreateDTO dto, int performedByUserId);
        Task DeleteAsync(int id);
        Task<IEnumerable<SaleDTO>> GetAllAsync();
        Task<IReadOnlyList<SaleNoteSummaryDTO>> GetAllNotesAsync(int saleId);
        Task<SaleDetailDTO?> GetByIdAsync(int id);
        Task<IReadOnlyList<SaleNoteSummaryDTO>> GetCreditNotesAsync(int saleId);
        Task<IReadOnlyList<SaleNoteSummaryDTO>> GetDebitNotesAsync(int saleId);
        Task<InvoiceBasicDTO> GetInvoiceAsync(int saleId);
        Task<InvoiceDetailDTO> GetInvoiceDetailAsync(int Id);
        Task RegisterStockWarningsAsync(IEnumerable<SaleStockWarningInputDTO> warnings);
        Task<SaleResolveStockWarningResultDTO> ResolveStockWarningAsync(int saleId, SaleResolveStockWarningDTO dto, int userId);
    }
}
