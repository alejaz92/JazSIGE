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

        public ISalesOrderRepository SalesOrderRepository { get; }
        public ISalesOrderArticleRepository SalesOrderArticleRepository { get; }




        public UnitOfWork(
            SalesDbContext context,
            IArticlePriceListRepository articlePriceListRepository,
            ISalesQuoteRepository salesQuoteRepository,
            ISalesQuoteArticleRepository salesQuoteArticleRepository,
            ISalesOrderRepository salesOrderRepository,
            ISalesOrderArticleRepository salesOrderArticleRepository
            )
        {
            _context = context;
            ArticlePriceListRepository = articlePriceListRepository;
            SalesQuoteRepository = salesQuoteRepository;
            SalesQuoteArticleRepository = salesQuoteArticleRepository;
            SalesOrderRepository = salesOrderRepository;
            SalesOrderArticleRepository = salesOrderArticleRepository;
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
