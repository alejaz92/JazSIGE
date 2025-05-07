using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories
{
    public class LineRepository : GenericRepository<Line>, ILineRepository
    {
        private readonly CatalogDbContext _dbContext;

        public LineRepository(CatalogDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Line>> GetByLineGroupIdAsync(int lineGroupId) => await _dbContext.Lines
                .Where(line => line.LineGroupId == lineGroupId)
                .Include(line => line.LineGroup)
                .ToListAsync();
    }
}
