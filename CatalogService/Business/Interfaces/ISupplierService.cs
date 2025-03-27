using CatalogService.Business.Models.Supplier;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface ISupplierService : IGenericService<Supplier, SupplierDTO, SupplierCreateDTO>
    {
    }
}
