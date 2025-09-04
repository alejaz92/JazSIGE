using SalesService.Business.Models.Clients;

namespace SalesService.Business.Interfaces.Clients
{
    public interface IFiscalServiceClient
    {
        //Task<FiscalDocumentResponseDTO> CreateCreditNoteAsync(CreditNoteCreateClientDTO dto);
        //Task<FiscalDocumentResponseDTO> CreateDebitNoteAsync(DebitNoteCreateClientDTO dto);
        Task<FiscalDocumentResponseDTO> CreateFiscalNoteAsync(FiscalDocumentCreateDTO dto);
        Task<FiscalDocumentResponseDTO?> GetByIdAsync(int id);
        Task<FiscalDocumentResponseDTO?> GetBySaleIdAsync(int salesOrderId);
        Task<IEnumerable<FiscalDocumentResponseDTO>> GetCreditNotesAsync(int saleId);
        Task<IEnumerable<FiscalDocumentResponseDTO>> GetDebitNotesAsync(int saleId);
    }
}
