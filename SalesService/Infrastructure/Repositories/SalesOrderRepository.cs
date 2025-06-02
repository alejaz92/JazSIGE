using CatalogService.Infrastructure.Repositories;
using SalesService.Infrastructure.Data;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.SalesOrder;

namespace SalesService.Infrastructure.Repositories
{
    public class SalesOrderRepository : GenericRepository<SalesOrder>, ISalesOrderRepository
    {
        private readonly SalesDbContext _context;

        public SalesOrderRepository(SalesDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
