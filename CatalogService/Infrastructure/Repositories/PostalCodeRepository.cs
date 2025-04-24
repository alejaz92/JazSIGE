using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories
{
    public class PostalCodeRepository : GenericRepository<PostalCode>, IPostalCodeRepository
    {
        private readonly CatalogDbContext _dbContext;

        public PostalCodeRepository(CatalogDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // get with includes by CityId
        public async Task<IEnumerable<PostalCode>> GetByCityIdAsync(int cityId)
        {
            return await _dbContext.PostalCodes
                .Include(p => p.City)
                .Include(p => p.City.Province)
                .Include(p => p.City.Province.Country)
                .Where(p => p.CityId == cityId)
                .ToListAsync();
        }
    }
}
