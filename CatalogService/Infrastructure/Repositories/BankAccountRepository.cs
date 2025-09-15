using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Infrastructure.Repositories
{
    public class BankAccountRepository : GenericRepository<BankAccount>, IBankAccountRepository
    {
        private readonly CatalogDbContext _dbContext;
        public BankAccountRepository(CatalogDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

    }
}
