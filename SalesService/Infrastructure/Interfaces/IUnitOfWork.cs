using SalesService.Infrastructure.Repositories;

namespace SalesService.Infrastructure.Interfaces
{
    public interface IUnitOfWork
    {
        IArticlePriceListRepository ArticlePriceListRepository { get; }

        ISalesQuoteRepository SalesQuoteRepository { get; }
        ISalesQuoteArticleRepository SalesQuoteArticleRepository { get; }

        //ISalesOrderRepository SalesOrderRepository { get; }
        //ISalesOrderArticleRepository SalesOrderArticleRepository { get; }

        ISaleRepository SaleRepository { get; }
        ISaleArticleRepository SaleArticleRepository { get; }


        Task SaveAsync();
    }
}
