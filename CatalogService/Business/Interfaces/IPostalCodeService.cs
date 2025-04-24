using CatalogService.Business.Models.PostalCode;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface IPostalCodeService : IGenericService<PostalCode, PostalCodeDTO, PostalCodeCreateDTO>
    {
        Task<IEnumerable<PostalCodeDTO>> GetByCityIdAsync(int cityId);
    }
}
