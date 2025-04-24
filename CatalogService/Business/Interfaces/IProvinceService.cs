using CatalogService.Business.Models.Province;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface IProvinceService : IGenericService<Province, ProvinceDTO, ProvinceCreateDTO>
    {
        Task<IEnumerable<ProvinceDTO>> GetByCountryIdAsync(int countryId);
    }
}
