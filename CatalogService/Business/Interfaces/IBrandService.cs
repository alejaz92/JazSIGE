using CatalogService.Business.Models.Brand;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface IBrandService : IGenericService<Brand, BrandDTO, BrandCreateDTO>
    {

    }
}
