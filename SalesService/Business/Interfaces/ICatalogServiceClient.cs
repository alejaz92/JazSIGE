using SalesService.Business.Models;

namespace SalesService.Business.Interfaces
{
    public interface ICatalogServiceClient
    {
        Task<IEnumerable<PriceListDTO>> GetPriceLists();
    }
}
