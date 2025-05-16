using SalesService.Infrastructure.Data;
using SalesService.Infrastructure.Interfaces;

namespace SalesService.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SalesDbContext _context;

        public IArticlePriceListRepository _articlePriceListRepository { get; }

        public IArticlePriceListRepository ArticlePriceListRepository => _articlePriceListRepository;


        public UnitOfWork(
            SalesDbContext context,
            IArticlePriceListRepository articlePriceListRepository
            )
        {
            _context = context;
            _articlePriceListRepository = articlePriceListRepository;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
