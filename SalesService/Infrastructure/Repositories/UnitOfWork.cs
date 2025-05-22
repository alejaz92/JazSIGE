using SalesService.Infrastructure.Data;
using SalesService.Infrastructure.Interfaces;

namespace SalesService.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SalesDbContext _context;

        public IArticlePriceListRepository ArticlePriceListRepository { get; }
        public ISalesQuoteRepository SalesQuoteRepository { get; }
        public ISalesQuoteArticleRepository SalesQuoteArticleRepository { get; }

     


        public UnitOfWork(
            SalesDbContext context,
            IArticlePriceListRepository articlePriceListRepository,
            ISalesQuoteRepository salesQuoteRepository,
            ISalesQuoteArticleRepository salesQuoteArticleRepository
            )
        {
            _context = context;
            ArticlePriceListRepository = articlePriceListRepository;
            SalesQuoteRepository = salesQuoteRepository;
            SalesQuoteArticleRepository = salesQuoteArticleRepository;
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
