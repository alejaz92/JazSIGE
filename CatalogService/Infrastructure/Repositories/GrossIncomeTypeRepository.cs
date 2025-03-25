using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Infrastructure.Repositories
{
    public class GrossIncomeTypeRepository : GenericRepository<GrossIncomeType>, IGrossIncomeTypeRepository
    {
        private readonly CatalogDbContext _dbContext;

        public GrossIncomeTypeRepository(CatalogDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
