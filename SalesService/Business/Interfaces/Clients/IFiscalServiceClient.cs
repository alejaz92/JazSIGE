using SalesService.Business.Models.Clients;

namespace SalesService.Business.Interfaces.Clients
{
    public interface IFiscalServiceClient
    {
        Task<FiscalDocumentResponseDTO> CreateCreditNoteAsync(CreditNoteCreateClientDTO dto);
        Task<FiscalDocumentResponseDTO> CreateDebitNoteAsync(DebitNoteCreateClientDTO dto);
        Task<FiscalDocumentResponseDTO> CreateInvoiceAsync(FiscalDocumentCreateDTO dto);
        Task<FiscalDocumentResponseDTO?> GetByIdAsync(int id);
        Task<FiscalDocumentResponseDTO?> GetBySaleIdAsync(int salesOrderId);
        Task<IEnumerable<FiscalDocumentResponseDTO>> GetCreditNotesByRelatedIdAsync(int relatedFiscalDocumentId);
        Task<IEnumerable<FiscalDocumentResponseDTO>> GetDebitNotesByRelatedIdAsync(int relatedFiscalDocumentId);
    }
}
