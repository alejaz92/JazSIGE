using CatalogService.Business.Models.Unit;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface IUnitService : IGenericService<Unit, UnitDTO, UnitCreateDTO>
    {
    }
}
