using SalesService.Infrastructure.Models;

namespace SalesService.Infrastructure.Interfaces
{
    public interface IArticlePriceListRepository : IGenericRepository<ArticlePriceList>
    {
        Task<ArticlePriceList?> GetCurrentPriceAsync(int articleId, int priceListId);
        Task<IEnumerable<ArticlePriceList>> GetCurrentPricesByArticleAsync(int articleId);
        Task<IEnumerable<ArticlePriceList>> GetPriceHistoryAsync(int articleId, int priceListId);
    }
}
