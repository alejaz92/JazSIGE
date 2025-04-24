using CatalogService.Infrastructure.Models;

namespace CatalogService.Infrastructure.Interfaces
{
    public interface IPostalCodeRepository : IGenericRepository<PostalCode>
    {
        Task<IEnumerable<PostalCode>> GetByCityIdAsync(int cityId);
    }
}
