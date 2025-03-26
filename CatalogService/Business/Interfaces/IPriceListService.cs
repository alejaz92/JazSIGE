using CatalogService.Business.Models.PriceList;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface IPriceListService : IGenericService<PriceList, PriceListDTO, PriceListCreateDTO>
    {
    }
}
