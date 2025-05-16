using SalesService.Business.Interfaces;
using SalesService.Business.Models;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models;

namespace SalesService.Business.Services
{
    public class ArticlePriceListService : IArticlePriceListService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ArticlePriceListService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ArticlePriceListDTO> CreateAsync(ArticlePriceListCreateDTO dto)
        {
            var model = new ArticlePriceList
            {
                ArticleId = dto.ArticleId,
                PriceListId = dto.PriceListId,
                Price = dto.Price,
                EffectiveFrom = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.ArticlePriceListRepository.AddAsync(model);
            await _unitOfWork.SaveAsync();

            return new ArticlePriceListDTO
            {
                Id = model.Id,
                ArticleId = model.ArticleId,
                PriceListId = model.PriceListId,
                Price = model.Price,
                EffectiveFrom = model.EffectiveFrom
            };
        }
        private static ArticlePriceListDTO MapToDTO(ArticlePriceList p)
        {
            return new ArticlePriceListDTO
            {
                Id = p.Id,
                ArticleId = p.ArticleId,
                PriceListId = p.PriceListId,
                Price = p.Price,
                EffectiveFrom = p.EffectiveFrom
            };
        }

        public async Task<IEnumerable<ArticlePriceListDTO>> GetCurrentPricesByArticleAsync(int articleId)
        {
            var list = await _unitOfWork.ArticlePriceListRepository.GetCurrentPricesByArticleAsync(articleId);
            return list.Select(MapToDTO);
        }

        public async Task<ArticlePriceListDTO?> GetCurrentPriceAsync(int articleId, int priceListId)
        {
            var price = await _unitOfWork.ArticlePriceListRepository.GetCurrentPriceAsync(articleId, priceListId);
            return price == null ? null : MapToDTO(price);
        }

        public async Task<IEnumerable<ArticlePriceListDTO>> GetPriceHistoryAsync(int articleId, int priceListId)
        {
            var history = await _unitOfWork.ArticlePriceListRepository.GetPriceHistoryAsync(articleId, priceListId);
            return history.Select(MapToDTO);
        }
    }
}
