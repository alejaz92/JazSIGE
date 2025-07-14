using FiscalDocumentationService.Infrastructure.Models;

namespace FiscalDocumentationService.Infrastructure.Interfaces
{
    public interface IFiscalDocumentRepository : IGenericRepository<FiscalDocument>
    {
        Task<FiscalDocument?> GetBySaleIdAsync(int saleId);
        Task<string?> GetLastDocumentNumberAsync(string documentType, string pointOfSale, string documentLetter);
    }
}
