using CatalogService.Business.Models.SellCondition;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface ISellConditionService : IGenericService<SellCondition, SellConditionDTO, SellConditionCreateDTO>
    {
    }
}
