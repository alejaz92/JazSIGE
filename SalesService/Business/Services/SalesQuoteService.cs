using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.SalesQuote;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.SalesQuote;

namespace SalesService.Business.Services
{
    public class SalesQuoteService : ISalesQuoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICatalogServiceClient _catalogServiceClient;
        private readonly IUserServiceClient _userServiceClient;
        private readonly ICompanyServiceClient _companyClient;

        public SalesQuoteService(
            IUnitOfWork unitOfWork, 
            ICatalogServiceClient catalogServiceClient, 
            IUserServiceClient userServiceClient,
            ICompanyServiceClient companyClient
            )
        {
            _unitOfWork = unitOfWork;
            _catalogServiceClient = catalogServiceClient;
            _userServiceClient = userServiceClient;
            _companyClient = companyClient;
        }

        public async Task<SalesQuoteDTO> CreateAsync(SalesQuoteCreateDTO dto)
        {
            // Obtener datos externos
            var customer = await _catalogServiceClient.GetCustomerByIdAsync(dto.CustomerId)
                ?? throw new Exception($"Customer {dto.CustomerId} not found");

            var seller = await _userServiceClient.GetUserByIdAsync(dto.SellerId)
                ?? throw new Exception($"Seller {dto.SellerId} not found");

            var transport = await _catalogServiceClient.GetTransportByIdAsync(dto.TransportId)
                ?? throw new Exception($"Transport {dto.TransportId} not found");

            var priceList = await _catalogServiceClient.GetPriceListByIdAsync(dto.PriceListId)
                ?? throw new Exception($"PriceList {dto.PriceListId} not found");

            var company = await _companyClient.GetCompanyInfoAsync()
                ?? throw new Exception("Company info not found");

            // Crear entidad
            var salesQuote = new SalesQuote
            {
                Date = dto.Date,
                ExpirationDate = dto.ExpirationDate,
                CustomerId = dto.CustomerId,
                SellerId = dto.SellerId,
                TransportId = dto.TransportId,
                PriceListId = dto.PriceListId,
                ExchangeRate = dto.ExchangeRate,
                Observations = dto.Observations
            };

            decimal subtotal = 0m, ivaTotal = 0m;
            var articleDTOs = new List<SalesQuoteArticleDTO>();

            foreach (var item in dto.Articles)
            {
                var article = await _catalogServiceClient.GetArticleByIdAsync(item.ArticleId)
                    ?? throw new Exception($"Article {item.ArticleId} not found");

                var unitPrice = item.UnitPriceUSD;
                var discount = unitPrice * (item.DiscountPercent / 100m);
                var priceAfterDiscount = unitPrice - discount;
                var itemSubtotal = priceAfterDiscount * item.Quantity;
                var itemIVA = itemSubtotal * (item.IVA / 100m);
                var totalUSD = itemSubtotal + itemIVA;

                var quoteArticle = new SalesQuote_Article
                {
                    ArticleId = item.ArticleId,
                    Quantity = item.Quantity,
                    UnitPriceUSD = unitPrice,
                    DiscountPercent = item.DiscountPercent,
                    IVA = item.IVA,
                    TotalUSD = totalUSD
                };

                salesQuote.Articles.Add(quoteArticle);
                subtotal += itemSubtotal;
                ivaTotal += itemIVA;

                articleDTOs.Add(new SalesQuoteArticleDTO
                {
                    ArticleId = item.ArticleId,
                    ArticleName = article.Description,
                    ArticleSKU = article.SKU,
                    Quantity = item.Quantity,
                    UnitPriceUSD = unitPrice,
                    DiscountPercent = item.DiscountPercent,
                    IVA = item.IVA,
                    TotalUSD = totalUSD
                });
            }

            salesQuote.SubtotalUSD = subtotal;
            salesQuote.IVAAmountUSD = ivaTotal;
            salesQuote.TotalUSD = subtotal + ivaTotal;

            await _unitOfWork.SalesQuoteRepository.AddAsync(salesQuote);
            await _unitOfWork.SaveAsync();

            // Devolver DTO enriquecido
            return new SalesQuoteDTO
            {
                Id = salesQuote.Id,
                Date = salesQuote.Date,
                ExpirationDate = salesQuote.ExpirationDate,

                CustomerId = customer.Id,
                CustomerName = customer.CompanyName,
                CustomerTaxID = customer.TaxId,
                CustomerAddress = customer.Address,
                CustomerPostalCode = customer.PostalCode,
                CustomerCity = customer.City,
                CustomerProvince = customer.Province,
                CustomerCountry = customer.Country,
                CustomerSellCondition = customer.SellCondition,

                SellerId = seller.Id,
                SellerName = $"{seller.FirstName} {seller.LastName}",

                TransportId = transport.Id,
                TransportName = transport.Name,
                TransportPostalCode = transport.PostalCode,
                TransportCity = transport.City,
                TransportProvince = transport.Province,
                TransportCountry = transport.Country,

                PriceListId = priceList.Id,
                PriceListName = priceList.Description,

                CompanyName = company.Name,
                CompanyTaxId = company.TaxId,
                CompanyAddress = company.Address,
                CompanyLogoUrl = company.LogoUrl,

                ExchangeRate = dto.ExchangeRate,
                SubtotalUSD = subtotal,
                IVAAmountUSD = ivaTotal,
                TotalUSD = subtotal + ivaTotal,
                Observations = dto.Observations,

                Articles = articleDTOs
            };
        }
        public async Task<SalesQuoteDTO> GetByIdAsync(int id)
        {
            var salesQuote = await _unitOfWork.SalesQuoteRepository.GetByIdWithDetailsAsync(id)
                ?? throw new Exception($"SalesQuote {id} not found");

            var customer = await _catalogServiceClient.GetCustomerByIdAsync(salesQuote.CustomerId)
                ?? throw new Exception($"Customer {salesQuote.CustomerId} not found");

            var seller = await _userServiceClient.GetUserByIdAsync(salesQuote.SellerId)
                ?? throw new Exception($"Seller {salesQuote.SellerId} not found");

            var transport = await _catalogServiceClient.GetTransportByIdAsync(salesQuote.TransportId)
                ?? throw new Exception($"Transport {salesQuote.TransportId} not found");

            var priceList = await _catalogServiceClient.GetPriceListByIdAsync(salesQuote.PriceListId)
                ?? throw new Exception($"PriceList {salesQuote.PriceListId} not found");

            var company = await _companyClient.GetCompanyInfoAsync()
                ?? throw new Exception("Company info not found");

            var articleDTOs = new List<SalesQuoteArticleDTO>();
            foreach (var item in salesQuote.Articles)
            {
                var article = await _catalogServiceClient.GetArticleByIdAsync(item.ArticleId)
                    ?? throw new Exception($"Article {item.ArticleId} not found");

                articleDTOs.Add(new SalesQuoteArticleDTO
                {
                    ArticleId = item.ArticleId,
                    ArticleName = article.Description,
                    ArticleSKU = article.SKU,
                    Quantity = item.Quantity,
                    UnitPriceUSD = item.UnitPriceUSD,
                    DiscountPercent = item.DiscountPercent,
                    IVA = item.IVA,
                    TotalUSD = item.TotalUSD
                });
            }

            return new SalesQuoteDTO
            {
                Id = salesQuote.Id,
                Date = salesQuote.Date,   
                ExpirationDate = salesQuote.ExpirationDate,

                CustomerId = customer.Id,
                CustomerName = customer.CompanyName,
                CustomerTaxID = customer.TaxId,
                CustomerAddress = customer.Address,
                CustomerPostalCode = customer.PostalCode,
                CustomerCity = customer.City,
                CustomerProvince = customer.Province,
                CustomerCountry = customer.Country,
                CustomerSellCondition = customer.SellCondition,

                SellerId = seller.Id,
                SellerName = $"{seller.FirstName} {seller.LastName}",

                TransportId = transport.Id,
                TransportName = transport.Name,
                TransportPostalCode = transport.PostalCode,
                TransportCity = transport.City,
                TransportProvince = transport.Province,
                TransportCountry = transport.Country,

                PriceListId = priceList.Id,
                PriceListName = priceList.Description,

                CompanyName = company.Name,
                CompanyTaxId = company.TaxId,
                CompanyAddress = company.Address,
                CompanyLogoUrl = company.LogoUrl,

                ExchangeRate = salesQuote.ExchangeRate,
                SubtotalUSD = salesQuote.SubtotalUSD,
                IVAAmountUSD = salesQuote.IVAAmountUSD,
                TotalUSD = salesQuote.TotalUSD,
                Observations = salesQuote.Observations,

                Articles = articleDTOs
            };
        }

        public async Task<IEnumerable<SalesQuoteListDTO>> GetAllAsync()
        {
            var quotes = await _unitOfWork.SalesQuoteRepository.GetAllIncludingAsync(q => q.Articles);

            var list = new List<SalesQuoteListDTO>();

            foreach (var q in quotes)
            {
                var customer = await _catalogServiceClient.GetCustomerByIdAsync(q.CustomerId);
                var seller = await _userServiceClient.GetUserByIdAsync(q.SellerId);

                list.Add(new SalesQuoteListDTO
                {
                    Id = q.Id,
                    Date = q.Date,
                    ExpirationDate = q.ExpirationDate,
                    CustomerName = customer?.CompanyName ?? "Unknown",
                    SellerName = seller != null ? $"{seller.FirstName} {seller.LastName}" : "Unknown",
                    TotalUSD = q.TotalUSD
                });
            }

            return list.OrderByDescending(x => x.Date);
        }
    }
}
