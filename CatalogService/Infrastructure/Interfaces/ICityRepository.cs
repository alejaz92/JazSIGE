using CatalogService.Infrastructure.Models;

namespace CatalogService.Infrastructure.Interfaces
{
    public interface ICityRepository : IGenericRepository<City>
    {
        Task<IEnumerable<City>> GetByProvinceIdAsync(int provinceId);
    }
}
