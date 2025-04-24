using CatalogService.Business.Models.City;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface ICityService : IGenericService<City, CityDTO, CityCreateDTO>
    {
        Task<IEnumerable<CityDTO>> GetByProvinceIdAsync(int provinceId);
    }
}
