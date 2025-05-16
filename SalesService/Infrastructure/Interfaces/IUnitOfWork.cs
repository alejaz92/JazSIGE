namespace SalesService.Infrastructure.Interfaces
{
    public interface IUnitOfWork
    {
        IArticlePriceListRepository ArticlePriceListRepository { get; }

        Task SaveAsync();
    }
}
