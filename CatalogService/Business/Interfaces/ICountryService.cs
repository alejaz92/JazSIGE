using CatalogService.Business.Models.Country;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface ICountryService : IGenericService<Country, CountryDTO, CountryCreateDTO>
    {
    }
}
