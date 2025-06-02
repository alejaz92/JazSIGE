namespace SalesService.Infrastructure.Interfaces
{
    public interface IUnitOfWork
    {
        IArticlePriceListRepository ArticlePriceListRepository { get; }

        ISalesQuoteRepository SalesQuoteRepository { get; }
        ISalesQuoteArticleRepository SalesQuoteArticleRepository { get; }

        ISalesOrderRepository SalesOrderRepository { get; }
        ISalesOrderArticleRepository SalesOrderArticleRepository { get; }


        Task SaveAsync();
    }
}
