using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Infrastructure.Repositories
{
    public class PostalCodeRepository : GenericRepository<PostalCode>, IPostalCodeRepository
    {
        private readonly CatalogDbContext _dbContext;

        public PostalCodeRepository(CatalogDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
