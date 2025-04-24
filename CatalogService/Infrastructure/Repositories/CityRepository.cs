using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories
{
    public class CityRepository : GenericRepository<City>, ICityRepository
    {
        private readonly CatalogDbContext _dbContext;

        public CityRepository(CatalogDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // get with includes by ProvinceId
        public async Task<IEnumerable<City>> GetByProvinceIdAsync(int provinceId)
        {
            return await _dbContext.Cities
                .Include(c => c.Province)
                .Where(c => c.ProvinceId == provinceId)
                .ToListAsync();
        }
    }
}
