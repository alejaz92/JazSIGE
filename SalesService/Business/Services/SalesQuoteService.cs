using SalesService.Business.Interfaces;
using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.Clients;
using SalesService.Business.Models.Sale;
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
            await ValidateCustomerBlockAsync(dto);
            await ValidateSellerAsync(dto.SellerId);

            var priceList = await _catalogServiceClient.GetPriceListByIdAsync(dto.PriceListId)
                ?? throw new Exception($"PriceList {dto.PriceListId} not found");

            var company = await _companyClient.GetCompanyInfoAsync()
                ?? throw new Exception("Company info not found");

            int newId;

            await using (var transaction = await _unitOfWork.SalesQuoteRepository.BeginTransactionAsync())
            {
                try
                {
                    var salesQuote = new SalesQuote
                    {
                        IsFinalConsumer = dto.IsFinalConsumer,
                        CustomerIdType = dto.CustomerIdType,
                        CustomerTaxId = dto.CustomerTaxId,
                        CustomerName = dto.CustomerName,
                        CustomerPostalCodeId = dto.CustomerPostalCodeId,
                        Date = dto.Date,
                        ExpirationDate = dto.ExpirationDate,
                        CustomerId = dto.CustomerId,
                        SellerId = dto.SellerId,
                        PriceListId = dto.PriceListId,
                        ExchangeRate = dto.ExchangeRate,
                        Observations = dto.Observations
                    };

                    decimal subtotal = 0m, ivaTotal = 0m;

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

                        salesQuote.Articles.Add(new SalesQuote_Article
                        {
                            ArticleId = item.ArticleId,
                            Quantity = item.Quantity,
                            UnitPriceUSD = unitPrice,
                            DiscountPercent = item.DiscountPercent,
                            IVA = item.IVA,
                            TotalUSD = totalUSD
                        });

                        subtotal += itemSubtotal;
                        ivaTotal += itemIVA;
                    }

                    salesQuote.SubtotalUSD = subtotal;
                    salesQuote.IVAAmountUSD = ivaTotal;
                    salesQuote.TotalUSD = subtotal + ivaTotal;

                    await _unitOfWork.SalesQuoteRepository.AddAsync(salesQuote);
                    await _unitOfWork.SaveAsync();

                    newId = salesQuote.Id;

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException("Error creating sales quote.", ex);
                }
            }

            // 👇 Ya fuera del alcance/using de la transacción
            return await GetByIdAsync(newId);
        }
        public async Task<SalesQuoteDTO> GetByIdAsync(int id)
        {
            var salesQuote = await _unitOfWork.SalesQuoteRepository.GetByIdWithDetailsAsync(id)
                ?? throw new Exception($"SalesQuote {id} not found");

            CustomerDTO? customer = null;
            string postalCode = string.Empty;

            if (!salesQuote.IsFinalConsumer)
            {
                if (!salesQuote.CustomerId.HasValue)
                    throw new InvalidOperationException(
                        "Sales quote has IsFinalConsumer = false but no CustomerId.");

                customer = await _catalogServiceClient.GetCustomerByIdAsync(salesQuote.CustomerId.Value)
                    ?? throw new Exception($"Customer {salesQuote.CustomerId} not found");

                postalCode = customer.PostalCode ?? string.Empty;
            }
            else
            {
                if (!salesQuote.CustomerPostalCodeId.HasValue)
                    throw new InvalidOperationException(
                        "Sales quote for final consumer has no CustomerPostalCodeId.");

                var postalDto = await _catalogServiceClient
                    .GetPostalCodeByIdAsync(salesQuote.CustomerPostalCodeId.Value)
                    ?? throw new Exception($"Postal code {salesQuote.CustomerPostalCodeId.Value} not found");

                postalCode = postalDto.Code;
            }

            await ValidateSellerAsync(salesQuote.SellerId);

            var priceList = await _catalogServiceClient.GetPriceListByIdAsync(salesQuote.PriceListId)
                ?? throw new Exception($"PriceList {salesQuote.PriceListId} not found");

            var seller = await _userServiceClient.GetUserByIdAsync(salesQuote.SellerId)
                ?? throw new Exception($"Seller {salesQuote.SellerId} not found");

            var company = await _companyClient.GetCompanyInfoAsync()
                ?? throw new Exception("Company info not found");

            var articleDTOs = new List<SalesQuoteArticleDTO>();
            foreach (var item in salesQuote.Articles ?? Enumerable.Empty<SalesQuote_Article>())
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
                IsFinalConsumer = salesQuote.IsFinalConsumer,

                CustomerId = salesQuote.CustomerId ?? 0,
                CustomerName = salesQuote.CustomerName,
                CustomerTaxID = salesQuote.CustomerTaxId,
                CustomerAddress = customer?.Address,
                CustomerPostalCode = customer?.PostalCode ?? postalCode,
                CustomerCity = customer?.City,
                CustomerProvince = customer?.Province,
                CustomerCountry = customer?.Country,
                CustomerSellCondition = customer?.SellCondition,
                CustomerIVAType = customer?.IVAType,
                CustomerPostalCodeId = salesQuote.CustomerPostalCodeId, // <— pasa el ID directo
                CustomerIdType = salesQuote.CustomerIdType,


                SellerId = salesQuote.SellerId,
                SellerName = seller.FirstName + " " + seller.LastName,

                PriceListId = priceList.Id,
                PriceListName = priceList.Description,

                CompanyName = company.Name,
                CompanyShortName = company.ShortName,
                CompanyTaxId = company.TaxId,
                CompanyAddress = company.Address,
                CompanyPostalCode = company.PostalCode,
                CompanyCity = company.City,
                CompanyProvince = company.Province,
                CompanyCountry = company.Country,
                CompanyPhone = company.Phone,
                CompanyEmail = company.Email,
                CompanyLogoUrl = company.LogoUrl,
                CompanyIVAType = company.IVAType,
                CompanyGrossIncome = company.GrossIncome,
                CompanyDateOfIncorporation = company.DateOfIncorporation,

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

                //var customer = await _catalogServiceClient.GetCustomerByIdAsync(q.CustomerId);
                var seller = await _userServiceClient.GetUserByIdAsync(q.SellerId);

                list.Add(new SalesQuoteListDTO
                {
                    Id = q.Id,
                    Date = q.Date,
                    ExpirationDate = q.ExpirationDate,
                    CustomerName = q.CustomerName,
                    SellerName = seller != null ? $"{seller.FirstName} {seller.LastName}" : "Unknown",
                    TotalUSD = q.TotalUSD,
                    ExchangeRate = q.ExchangeRate
                });
            }

            return list.OrderByDescending(x => x.Date);
        }   
        private async Task ValidateCustomerBlockAsync(SalesQuoteCreateDTO dto)
        {
            if (!dto.IsFinalConsumer)
            {
                var customer = await _catalogServiceClient.GetCustomerByIdAsync(dto.CustomerId!.Value);
                if (customer == null) throw new ArgumentException("Invalid customer ID.");

                dto.CustomerPostalCodeId = customer.PostalCodeId;
                dto.CustomerIdType = "CUIT";
                dto.CustomerTaxId = customer.TaxId;
                dto.CustomerName = customer.CompanyName;
                return;
            }

            // Final consumer:
            if (dto.CustomerPostalCodeId == null)
                throw new ArgumentException("Postal code is required for final consumer sales.");

            var postal = await _catalogServiceClient.GetPostalCodeByIdAsync(dto.CustomerPostalCodeId.Value);
            if (postal == null) throw new ArgumentException("Invalid postal code ID.");

            if (dto.CustomerIdType != "DNI" && dto.CustomerIdType != "CUIT" && dto.CustomerIdType != "CUIL")
                throw new ArgumentException("Customer ID type must be DNI, CUIT or CUIL for final consumer sales.");

            if (string.IsNullOrWhiteSpace(dto.CustomerTaxId))
                throw new ArgumentException("Customer tax ID is required for final consumer sales.");
        }
        private async Task ValidateSellerAsync(int sellerId)
        {
            var user = await _userServiceClient.GetUserByIdAsync(sellerId);
            if (user == null) throw new ArgumentException("Invalid seller ID.");
        }
    }
}
