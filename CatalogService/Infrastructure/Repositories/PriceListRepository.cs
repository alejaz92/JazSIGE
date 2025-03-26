using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Infrastructure.Repositories
{
    public class PriceListRepository : GenericRepository<PriceList>, IPriceListRepository
    {
        private readonly CatalogDbContext _dbContext;

        public PriceListRepository(CatalogDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
