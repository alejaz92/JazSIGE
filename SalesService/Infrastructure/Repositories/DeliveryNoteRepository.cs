using CatalogService.Infrastructure.Repositories;
using SalesService.Infrastructure.Data;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.Sale;

namespace SalesService.Infrastructure.Repositories
{
    public class DeliveryNoteRepository : GenericRepository<DeliveryNote>, IDeliveryNoteRepository
    {
        private readonly SalesDbContext _context;

        public DeliveryNoteRepository(SalesDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
