using CatalogService.Infrastructure.Repositories;
using SalesService.Infrastructure.Data;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.Sale;

namespace SalesService.Infrastructure.Repositories
{
    public class DeliveryNoteArticleRepository : GenericRepository<DeliveryNote_Article>, IDeliveryNoteArticleRepository
    {
        private readonly SalesDbContext _context;

        public DeliveryNoteArticleRepository(SalesDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
