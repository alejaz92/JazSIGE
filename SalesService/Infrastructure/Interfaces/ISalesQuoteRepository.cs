using SalesService.Infrastructure.Models.SalesQuote;

namespace SalesService.Infrastructure.Interfaces
{
    public interface ISalesQuoteRepository : IGenericRepository<SalesQuote>    
    {
        Task<SalesQuote?> GetByIdWithDetailsAsync(int id);
    }
}
