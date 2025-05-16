using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SalesService.Infrastructure.Data;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models;

namespace SalesService.Infrastructure.Repositories
{
    public class ArticlePriceListRepository : GenericRepository<ArticlePriceList>, IArticlePriceListRepository
    {
        private readonly SalesDbContext _context;

        public ArticlePriceListRepository(SalesDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ArticlePriceList?> GetCurrentPriceAsync(int articleId, int priceListId)
        {
            return await _context.ArticlePriceLists
                .Where(p => p.ArticleId == articleId && p.PriceListId == priceListId && p.IsActive)
                .OrderByDescending(p => p.EffectiveFrom)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ArticlePriceList>> GetCurrentPricesByArticleAsync(int articleId)
        {
            return await _context.ArticlePriceLists
                .Where(p => p.ArticleId == articleId && p.IsActive)
                .GroupBy(p => p.PriceListId)
                .Select(g => g.OrderByDescending(x => x.EffectiveFrom).First())
                .ToListAsync();
        }

        public async Task<IEnumerable<ArticlePriceList>> GetPriceHistoryAsync(int articleId, int priceListId)
        {
            return await _context.ArticlePriceLists
                .Where(p => p.ArticleId == articleId && p.PriceListId == priceListId)
                .OrderByDescending(p => p.EffectiveFrom)
                .ToListAsync();
        }
    }
}
