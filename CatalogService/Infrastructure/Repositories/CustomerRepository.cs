using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Infrastructure.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        private readonly CatalogDbContext _dbContext;
        public CustomerRepository(CatalogDbContext dbContext) : base(dbContext) 
        {
            _dbContext = dbContext;
        }
    }
}
