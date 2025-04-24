using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories
{
    public class ProvinceRepository : GenericRepository<Province>, IProvinceRepository
    {
        private readonly CatalogDbContext _dbContext;
        public ProvinceRepository(CatalogDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // get with includes by CountryId
        public async Task<IEnumerable<Province>> GetByCountryIdAsync(int countryId)
        {
            return await _dbContext.Provinces
                .Include(p => p.Country)
                .Where(p => p.CountryId == countryId)
                .ToListAsync();
        }
    }
}
