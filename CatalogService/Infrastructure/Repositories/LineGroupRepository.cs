using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Infrastructure.Repositories
{
    public class LineGroupRepository : GenericRepository<LineGroup>, ILineGroupRepository
    {
        private readonly CatalogDbContext _dbContext;

        public LineGroupRepository(CatalogDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
