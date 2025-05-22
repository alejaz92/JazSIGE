using SalesService.Business.Models.Article_PriceLists;

namespace SalesService.Business.Interfaces
{
    public interface IArticlePriceListService
    {
        Task<ArticlePriceListDTO> CreateAsync(ArticlePriceListCreateDTO dto);
        Task<ArticlePriceListDTO?> GetCurrentPriceAsync(int articleId, int priceListId);
        Task<IEnumerable<ArticlePriceListDTO>> GetCurrentPricesByArticleAsync(int articleId);
        Task<IEnumerable<ArticlePriceListDTO>> GetPriceHistoryAsync(int articleId, int priceListId);
    }
}
