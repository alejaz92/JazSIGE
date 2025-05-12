
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Business.Interfaces;
using CatalogService.Infrastructure.Models;


namespace CatalogService.Business.Services
{
    public class ArticleValidatorService : IArticleValidatorService
    {
        private readonly IArticleRepository _articleRepository;

        public ArticleValidatorService(IArticleRepository articleRepository)
        {
            _articleRepository = articleRepository;
        }

        public async Task<int> ActiveArticlesByBrand(int brandId)
        {
            var articles = await _articleRepository.FindAsync(a => a.BrandId == brandId && a.IsActive);
            return articles.Count();
        }

        public async Task<int> ActiveArticlesByLine(int lineId)
        {
            var articles = await _articleRepository.FindAsync(a => a.LineId == lineId && a.IsActive);
            return articles.Count();
        }

        public async Task<int> ActiveArticlesByUnit(int unitId)
        {
            var articles = await _articleRepository.FindAsync(a => a.UnitId == unitId && a.IsActive);
            return articles.Count();
        }

        public async Task<int> ActiveArticlesByGrossIncomeType(int grossIncomeTypeId)
        {
            var articles = await _articleRepository.FindAsync(a => a.GrossIncomeTypeId == grossIncomeTypeId && a.IsActive);
            return articles.Count();
        }
    }
}