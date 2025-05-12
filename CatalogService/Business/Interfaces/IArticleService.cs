using CatalogService.Business.Models.Article;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface IArticleService : IGenericService<Article, ArticleDTO, ArticleCreateDTO>
    {
        Task<bool> ArticleExistsByDescription(string description);
        Task<bool> ArticleExistsBySKU(string sku);
        Task<ArticleDTO?> UpdateVisibilityAsync(int id);
    }
}
