using CatalogService.Business.Models.Warehouse;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface IWarehouseService : IGenericService<Warehouse, WarehouseDTO, WarehouseCreateDTO>
    {
    }
}
