using SalesService.Business.Interfaces;
using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.Sale;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.Sale;

namespace SalesService.Business.Services
{
    public class SaleService : ISaleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICatalogServiceClient _catalogService;
        private readonly IUserServiceClient _userService;

        public SaleService(
            IUnitOfWork unitOfWork,
            ICatalogServiceClient catalogService,
            IUserServiceClient userService)
        {
            _unitOfWork = unitOfWork;
            _catalogService = catalogService;
            _userService = userService;
        }

        public async Task<IEnumerable<SaleDTO>> GetAllAsync()
        {
            var sales = await _unitOfWork.SaleRepository.GetAllIncludingAsync(s => s.Articles);
            var result = new List<SaleDTO>();

            foreach (var sale in sales)
            {
                var customer = await _catalogService.GetCustomerByIdAsync(sale.CustomerId);
                var seller = await _userService.GetUserByIdAsync(sale.SellerId);

                var subtotal = sale.Articles.Sum(a =>
                    a.UnitPrice * a.Quantity * (1 - a.DiscountPercent / 100));

                var ivaTotal = sale.Articles.Sum(a =>
                {
                    var baseAmount = a.UnitPrice * a.Quantity * (1 - a.DiscountPercent / 100);
                    return baseAmount * (a.IVAPercent / 100);
                });

                result.Add(new SaleDTO
                {
                    Id = sale.Id,
                    CustomerName = customer.CompanyName,
                    SellerName = $"{seller.FirstName} {seller.LastName}",
                    Date = sale.Date,
                    Total = Math.Round(subtotal + ivaTotal, 2)
                });
            }

            return result;
        }

        public async Task<SaleDetailDTO?> GetByIdAsync(int id)
        {
            var sale = await _unitOfWork.SaleRepository.GetIncludingAsync(id, s => s.Articles);
            if (sale == null) return null;

            var customer = await _catalogService.GetCustomerByIdAsync(sale.CustomerId);
            var seller = await _userService.GetUserByIdAsync(sale.SellerId);

            var articleDTOs = new List<SaleArticleDetailDTO>();

            foreach (var article in sale.Articles)
            {
                var articleInfo = await _catalogService.GetArticleByIdAsync(article.ArticleId);

                articleDTOs.Add(new SaleArticleDetailDTO
                {
                    ArticleId = article.ArticleId,
                    ArticleName = articleInfo.Description,
                    Quantity = article.Quantity,
                    UnitPrice = article.UnitPrice,
                    DiscountPercent = article.DiscountPercent,
                    IVAPercent = article.IVAPercent
                });
            }

            return new SaleDetailDTO
            {
                Id = sale.Id,
                Date = sale.Date,
                ExchangeRate = sale.ExchangeRate,
                Observations = sale.Observations,
                CustomerName = customer.CompanyName,
                SellerName = $"{seller.FirstName} {seller.LastName}",
                Articles = articleDTOs
            };
        }

        public async Task<SaleDetailDTO> CreateAsync(SaleCreateDTO dto)
        {
            var sale = new Sale
            {
                CustomerId = dto.CustomerId,
                SellerId = dto.SellerId,
                Date = dto.Date,
                ExchangeRate = dto.ExchangeRate,
                Observations = dto.Observations,
                Articles = dto.Articles.Select(a => new Sale_Article
                {
                    ArticleId = a.ArticleId,
                    Quantity = a.Quantity,
                    UnitPrice = a.UnitPrice,
                    DiscountPercent = a.DiscountPercent,
                    IVAPercent = a.IVAPercent
                }).ToList()
            };

            await _unitOfWork.SaleRepository.AddAsync(sale);
            await _unitOfWork.SaveAsync();

            return await GetByIdAsync(sale.Id) ?? throw new Exception("sale not available.");
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.SaleRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }
    
    }
}
