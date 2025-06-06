using CatalogService.Infrastructure.Repositories;
using SalesService.Infrastructure.Data;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.Sale;

namespace SalesService.Infrastructure.Repositories
{
    public class SaleArticleRepository : GenericRepository<Sale_Article>, ISaleArticleRepository
    {
        private readonly SalesDbContext _context;

        public SaleArticleRepository(SalesDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
