using CatalogService.Business.Models.LineGroup;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface ILineGroupService : IGenericService<LineGroup, LineGroupDTO, LineGroupCreateDTO>
    {

    }
}
