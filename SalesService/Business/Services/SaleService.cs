using SalesService.Business.Interfaces;
using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.Clients;
using SalesService.Business.Models.Sale;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.Sale;

namespace SalesService.Business.Services
{
    public class SaleService : ISaleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICatalogServiceClient _catalogService;
        private readonly IStockServiceClient _stockService;
        private readonly IUserServiceClient _userService;

        public SaleService(
            IUnitOfWork unitOfWork,
            ICatalogServiceClient catalogService,
            IStockServiceClient stockService,
            IUserServiceClient userService)
        {
            _unitOfWork = unitOfWork;
            _catalogService = catalogService;
            _stockService = stockService;
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
                    Total = Math.Round(subtotal + ivaTotal, 2),
                    ExchangeRate = sale.ExchangeRate,
                    HasInvoice = sale.HasInvoice,
                    IsFullyDelivered = sale.IsFullyDelivered
                });
            }

            return result;
        }

        public async Task<SaleDetailDTO?> GetByIdAsync(int id)
        {
            var sale = await _unitOfWork.SaleRepository.GetIncludingAsync(id, s => s.Articles);
            if (sale == null) return null;

            var deliveryNotes = await _unitOfWork.DeliveryNoteRepository.FindIncludingAsync(
                dn => dn.SaleId == sale.Id,
                dn => dn.Articles
            );

            var hasDeliveryNotes = deliveryNotes.Any();            


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
                    ArticleSKU = articleInfo.SKU,
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

                CustomerId = customer.Id,
                CustomerName = customer.CompanyName,
                CustomerTaxID = customer.TaxId,
                CustomerAddress = customer.Address,
                CustomerPostalCode = customer.PostalCode,
                CustomerCity = customer.City,
                CustomerProvince = customer.Province,
                CustomerCountry = customer.Country,
                CustomerSellCondition = customer.SellCondition,
                CustomerIVAType = customer.IVAType,

                SellerId = sale.SellerId,
                SellerName = $"{seller.FirstName} {seller.LastName}",

                Articles = articleDTOs,
                HasInvoice = sale.HasInvoice,
                HasDeliveryNotes = hasDeliveryNotes,
                IsFullyDelivered = sale.IsFullyDelivered
            };
        }

        public async Task<SaleDetailDTO> CreateAsync(SaleCreateDTO dto)
        {
            // validate user
            var user = await _userService.GetUserByIdAsync(dto.SellerId);
            if (user == null)
                throw new ArgumentException("Invalid seller ID.");

            // validate customer
            var customer = await _catalogService.GetCustomerByIdAsync(dto.CustomerId);
            if (customer == null)
                throw new ArgumentException("Invalid customer ID.");

            // validate articles
            foreach( var article in dto.Articles)
            {
                var articleInfo = await _catalogService.GetArticleByIdAsync(article.ArticleId);
                if (articleInfo == null)
                    throw new ArgumentException($"Invalid article ID: {article.ArticleId}");
                if (article.Quantity <= 0)
                    throw new ArgumentException($"Invalid quantity for article {article.ArticleId}.");
            }


            using var transaction = await _unitOfWork.SaleRepository.BeginTransactionAsync();


            try
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

                await UpdateStockCommited(sale.Id, dto.Articles);

                await transaction.CommitAsync();

                return await GetByIdAsync(sale.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Error creating sale.", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.SaleRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }

        private async Task UpdateStockCommited(int saleId, List<SaleArticleCreateDTO> articles)
        {
            foreach (var article in articles)
            {
                var commitedEntry = new CommitedStockEntryCreateDTO
                {
                    SaleId = saleId,
                    ArticleId = article.ArticleId,
                    Quantity = article.Quantity
                };
                await _stockService.RegisterCommitedStockAsync(commitedEntry);
            }
        }

    }
}
