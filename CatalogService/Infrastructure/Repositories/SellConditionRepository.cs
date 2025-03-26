using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Infrastructure.Repositories
{
    public class SellConditionRepository : GenericRepository<SellCondition>, ISellConditionRepository
    {
        private readonly CatalogDbContext _dbContext;
        public SellConditionRepository(CatalogDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
