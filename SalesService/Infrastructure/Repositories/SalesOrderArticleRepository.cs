using CatalogService.Infrastructure.Repositories;
using SalesService.Infrastructure.Data;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.SalesOrder;

namespace SalesService.Infrastructure.Repositories
{
    public class SalesOrderArticleRepository : GenericRepository<SalesOrder_Article>, ISalesOrderArticleRepository
    {
        private readonly SalesDbContext _context;

        public SalesOrderArticleRepository(SalesDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
