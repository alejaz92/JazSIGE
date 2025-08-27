using SalesService.Business.Models.Clients;
using SalesService.Business.Models.Sale;
using SalesService.Business.Models.Sale.fiscalDocs;

namespace SalesService.Business.Interfaces
{
    public interface ISaleService
    {
        Task<SaleDetailDTO> CreateAsync(SaleCreateDTO dto);
        Task<InvoiceBasicDTO> CreateCreditNoteAsync(int saleId, CreditNoteCreateForSaleDTO dto);
        Task<InvoiceBasicDTO> CreateDebitNoteAsync(int saleId, DebitNoteCreateForSaleDTO dto);
        Task<InvoiceBasicDTO> CreateInvoiceAsync(int saleId);
        Task<QuickSaleResultDTO> CreateQuickAsync(QuickSaleCreateDTO dto, int performedByUserId);
        Task DeleteAsync(int id);
        Task<IEnumerable<SaleDTO>> GetAllAsync();
        Task<SaleDetailDTO?> GetByIdAsync(int id);
        Task<InvoiceBasicDTO> GetInvoiceAsync(int saleId);
        Task<InvoiceDetailDTO> GetInvoiceDetailAsync(int saleId);
    }
}
