using SalesService.Business.Models.Clients;
using SalesService.Business.Models.Sale;

namespace SalesService.Business.Interfaces
{
    public interface ISaleService
    {
        Task<SaleDetailDTO> CreateAsync(SaleCreateDTO dto);
        Task<InvoiceBasicDTO> CreateInvoiceAsync(int saleId);
        Task<QuickSaleResultDTO> CreateQuickAsync(QuickSaleCreateDTO dto, int performedByUserId);
        Task DeleteAsync(int id);
        Task<IEnumerable<SaleDTO>> GetAllAsync();
        Task<SaleDetailDTO?> GetByIdAsync(int id);
        Task<InvoiceBasicDTO> GetInvoiceAsync(int saleId);
        Task<InvoiceDetailDTO> GetInvoiceDetailAsync(int saleId);
    }
}
