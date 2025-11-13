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
        public ISaleRepository SaleRepository { get; }
        public ISaleArticleRepository SaleArticleRepository { get; }

        public IDeliveryNoteRepository DeliveryNoteRepository { get; }
        public IDeliveryNoteArticleRepository DeliveryNoteArticleRepository { get; }
        public ISaleStockWarningRepository SaleStockWarningRepository { get; }




        public UnitOfWork(
            SalesDbContext context,
            IArticlePriceListRepository articlePriceListRepository,
            ISalesQuoteRepository salesQuoteRepository,
            ISalesQuoteArticleRepository salesQuoteArticleRepository,
            ISaleRepository saleRepository,
            ISaleArticleRepository saleArticleRepository,
            IDeliveryNoteRepository deliveryNoteRepository,
            IDeliveryNoteArticleRepository deliveryNoteArticleRepository,
            ISaleStockWarningRepository saleStockWarningRepository
            )
        {
            _context = context;
            ArticlePriceListRepository = articlePriceListRepository;
            SalesQuoteRepository = salesQuoteRepository;
            SalesQuoteArticleRepository = salesQuoteArticleRepository;
            SaleRepository = saleRepository;
            SaleArticleRepository = saleArticleRepository;
            DeliveryNoteRepository = deliveryNoteRepository;
            DeliveryNoteArticleRepository = deliveryNoteArticleRepository;
            SaleStockWarningRepository = saleStockWarningRepository;
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
