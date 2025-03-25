using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Infrastructure.Repositories
{
    public class IVATypeRepository : GenericRepository<IVAType>, IIVATypeRepository
    {
        private readonly CatalogDbContext _dbContext;

        public IVATypeRepository(CatalogDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
