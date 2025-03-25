using CatalogService.Business.Models.IVAType;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface IIVATypeService : IGenericService<IVAType, IVATypeDTO, IVATypeCreateDTO>
    {
    }
}
