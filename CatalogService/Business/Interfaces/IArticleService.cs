using CatalogService.Business.Models.Article;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface IArticleService : IGenericService<Article, ArticleDTO, ArticleCreateDTO>
    {
        Task<ArticleDTO?> UpdateVisibilityAsync(int id);
    }
}
