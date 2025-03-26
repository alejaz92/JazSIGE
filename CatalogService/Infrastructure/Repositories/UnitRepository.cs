using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Infrastructure.Repositories
{
    public class UnitRepository : GenericRepository<Unit>, IUnitRepository
    {
        private readonly CatalogDbContext _dbContext;
        public UnitRepository(CatalogDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
