
using StockService.Business.Models.Clients;

namespace StockService.Business.Interfaces
{
    public interface ISalesServiceClient
    {
        Task SendStockWarningsAsync(IEnumerable<SaleStockWarningInputDTO> warnings);
    }
}
