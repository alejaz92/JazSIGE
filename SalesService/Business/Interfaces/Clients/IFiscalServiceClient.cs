using SalesService.Business.Models.Clients;

namespace SalesService.Business.Interfaces.Clients
{
    public interface IFiscalServiceClient
    {
        Task<FiscalDocumentResponseDTO> CreateInvoiceAsync(FiscalDocumentCreateDTO dto);
    }
}
