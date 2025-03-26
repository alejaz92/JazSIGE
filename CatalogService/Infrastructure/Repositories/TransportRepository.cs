using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Infrastructure.Repositories
{
    public class TransportRepository : GenericRepository<Transport>, ITransportRepository
    {
        private readonly CatalogDbContext _dbContext;

        public TransportRepository(CatalogDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
