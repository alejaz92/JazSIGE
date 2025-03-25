using CatalogService.Business.Models.GrossIncomeType;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface IGrossIncomeTypeService : IGenericService<GrossIncomeType, GrossIncomeTypeDTO, GrossIncomeTypeCreateDTO>
    {
    }
}
