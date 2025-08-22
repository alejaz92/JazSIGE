using SalesService.Business.Interfaces;
using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.Clients;
using SalesService.Business.Models.DeliveryNote;
using SalesService.Business.Models.Sale;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.Sale;


namespace SalesService.Business.Services
{
    public class SaleService : ISaleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICatalogServiceClient _catalogService;
        private readonly IStockServiceClient _stockServiceClient;
        private readonly IUserServiceClient _userService;
        private readonly IFiscalServiceClient _fiscalServiceClient;
        private readonly IDeliveryNoteService _deliveryNoteService;
        private readonly ICompanyServiceClient _companyServiceClient;

        public SaleService(
            IUnitOfWork unitOfWork,
            ICatalogServiceClient catalogService,
            IStockServiceClient stockServiceClient,
            IUserServiceClient userService,
            IFiscalServiceClient fiscalServiceClient,
            IDeliveryNoteService deliveryNoteService,
            ICompanyServiceClient companyServiceClient)
        {
            _unitOfWork = unitOfWork;
            _catalogService = catalogService;
            _stockServiceClient = stockServiceClient;
            _userService = userService;
            _fiscalServiceClient = fiscalServiceClient;
            _deliveryNoteService = deliveryNoteService;
            _companyServiceClient = companyServiceClient;
        }

        public async Task<IEnumerable<SaleDTO>> GetAllAsync()
        {
            var sales = await _unitOfWork.SaleRepository.GetAllIncludingAsync(s => s.Articles);
            var result = new List<SaleDTO>();

            foreach (var sale in sales)
            {
                // if sale.customerId is not null




                if (!sale.IsFinalConsumer && sale.CustomerId.HasValue)
                {
                    var customer = await _catalogService.GetCustomerByIdAsync(sale.CustomerId.Value);
                    sale.CustomerName = customer?.CompanyName ?? "N/A";

                }       
                
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
                    IsFinalConsumer = sale.IsFinalConsumer,
                    CustomerName = sale.CustomerName,
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

            CustomerDTO customer = new CustomerDTO
            {
                Id = 0,
                CompanyName = "Final Consumer",
                TaxId = "N/A",
                Address = "N/A",
                PostalCode = "N/A",
                City = "N/A",
                Province = "N/A",
                Country = "N/A",
                SellCondition = "N/A",
                IVAType = "N/A"
            };

            if (!sale.IsFinalConsumer && sale.CustomerId.HasValue)
            {
                customer = await _catalogService.GetCustomerByIdAsync(sale.CustomerId.Value);
            }
            else
            {
                var postalCode = await _catalogService.GetPostalCodeByIdAsync(sale.CustomerPostalCodeId.Value);

                customer.PostalCode = postalCode?.Code ?? "N/A";
                customer.Province = postalCode?.Province ?? "N/A";
                customer.Country = postalCode?.Country ?? "N/A";
                customer.City = postalCode?.City ?? "N/A";
                customer.CompanyName = sale.CustomerName ?? "Final Consumer";
                customer.IVAType = "Consumidor Final";
                customer.TaxId = sale.CustomerTaxId ?? "N/A";
                customer.SellCondition = "Contado";
            }


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
                IsFinalConsumer = sale.IsFinalConsumer,

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
                DeliveryNotes = deliveryNotes.Select(dn => new DeliveryNoteDTO
                {
                    Id = dn.Id,
                    Code = dn.Code!,
                    Date = dn.Date,
                    Observations = dn.Observations,
                    Articles = dn.Articles.Select(a => new DeliveryNoteArticleDTO
                    {
                        ArticleId = a.ArticleId,
                        Quantity = a.Quantity,
                        DispatchCode = a.DispatchCode
                    }).ToList()
                }).ToList(),

                HasInvoice = sale.HasInvoice,
                HasDeliveryNotes = hasDeliveryNotes,
                IsFullyDelivered = sale.IsFullyDelivered

                
            };
        }
        public async Task<SaleDetailDTO> CreateAsync(SaleCreateDTO dto)
        {

            await ValidateSellerAsync(dto.SellerId);
            await ValidateCustomerBlockAsync(dto);
            await ValidateArticlesAndStockAsync(dto);


            using var transaction = await _unitOfWork.SaleRepository.BeginTransactionAsync();
            try
            {
                var sale = new Sale
                {
                    IsFinalConsumer = dto.IsFinalConsumer,
                    CustomerIdType = dto.CustomerIdType,
                    CustomerTaxId = dto.CustomerTaxId,
                    CustomerName = dto.CustomerName,
                    CustomerId = dto.CustomerId,
                    CustomerPostalCodeId = dto.CustomerPostalCodeId,
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

                await UpdateStockToCommited(sale.Id, dto.Articles, dto.CustomerId, dto.CustomerName);

                await transaction.CommitAsync();

                return await GetByIdAsync(sale.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Error creating sale.", ex);
            }
        }
        public async Task<QuickSaleResultDTO> CreateQuickAsync(QuickSaleCreateDTO dto, int performedByUserId)
        {
            // 1) Validaciones iguales a CreateAsync (usuario, cliente / final consumer, artículos, stock, etc.)
            // Reuso la lógica existente para que no diverja.
            await ValidateSellerAsync(dto.SellerId);
            await ValidateCustomerBlockAsync(dto);     // (ver helpers abajo)
            await ValidateArticlesAndStockAsync(dto);  // (ver helpers abajo)

            using var tx = await _unitOfWork.SaleRepository.BeginTransactionAsync();
            try
            {
                // 2) Crear la venta (igual a CreateAsync pero SIN registrar committed stock)
                var sale = new Sale
                {
                    IsFinalConsumer = dto.IsFinalConsumer,
                    CustomerIdType = dto.CustomerIdType,
                    CustomerTaxId = dto.CustomerTaxId,
                    CustomerName = dto.CustomerName,        // puede venir null en rápida; ok
                    CustomerId = dto.CustomerId,
                    CustomerPostalCodeId = dto.CustomerPostalCodeId,
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

                // 3) Crear Remito por la totalidad (descarga stock)
                // 3.1) Armado del DTO de remito
    
                var deliveryNoteCreateDTO = new DeliveryNoteCreateDTO
                {
                    SaleId = sale.Id,
                    Date = dto.Date,
                    Observations = dto.Observations,
                    Articles = dto.Articles.Select(a => new DeliveryNoteArticleCreateDTO
                    {
                        ArticleId = a.ArticleId,
                        Quantity = a.Quantity
                    }).ToList(),
                    Code = $"QSDN-{sale.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    WarehouseId = dto.WarehouseId,
                    TransportId = null,
                    DeclaredValue = 0,
                    NumberOfPackages = 0
                };
                // 3.2) Crear el remito
                var deliveryNote = await _deliveryNoteService.CreateQuickAsync(performedByUserId, deliveryNoteCreateDTO);



                sale.IsFullyDelivered = true;


                // 4) Emitir Factura
                var invoice = await CreateInvoiceAsync(sale.Id);


                // 5) Marcar flags (por si tu lógica interna no lo hizo ya)
                sale.HasInvoice = true;
                
                _unitOfWork.SaleRepository.Update(sale);
                await _unitOfWork.SaveAsync();

                await tx.CommitAsync();

                // 6) Devolver todo junto
                var saleDetail = await GetByIdAsync(sale.Id);
                return new QuickSaleResultDTO
                {
                    Sale = saleDetail!,
                    //DeliveryNote = deliveryNote,
                    Invoice = invoice
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw; // deja que el controller maneje el error
            }
        }
        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.SaleRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }
        private async Task UpdateStockToCommited(int saleId, List<SaleArticleCreateDTO> articles, int? customerId, string customerName)
        {
            foreach (var article in articles)
            {
                var commitedEntry = new CommitedStockEntryCreateDTO
                {
                    SaleId = saleId,
                    CustomerId =  customerId,
                    CustomerName = customerName,
                    ArticleId = article.ArticleId,
                    Quantity = article.Quantity
                };
                await _stockServiceClient.RegisterCommitedStockAsync(commitedEntry);
            }
        }              
        public async Task<InvoiceBasicDTO> CreateInvoiceAsync(int saleId)
        {
            var sale = await _unitOfWork.SaleRepository.GetIncludingAsync(saleId, s => s.Articles);
            if (sale == null)
                throw new InvalidOperationException("Sale not found.");

            if (sale.HasInvoice)
                throw new InvalidOperationException("Invoice already generated for this sale.");


            if (!sale.IsFinalConsumer && !sale.CustomerId.HasValue)
                throw new InvalidOperationException("Customer ID is required for non-final consumer sales.");

            if (!sale.IsFullyDelivered)
                throw new InvalidOperationException("Sale must be fully delivered before generating an invoice.");

            // get delivery notes using _deliveryNoteService.GetAllBySaleIdAsync
            var deliveryNotes = await _deliveryNoteService.GetAllBySaleIdAsync(sale.Id);


            CustomerDTO customer = new CustomerDTO
            {
                Id = 0,
                CompanyName = "Final Consumer",
                TaxId = "N/A",
                Address = "N/A",
                PostalCode = "N/A",
                City = "N/A",
                Province = "N/A",
                Country = "N/A",
                SellCondition = "N/A",
                IVAType = "N/A"
            };

            if (!sale.IsFinalConsumer)
            {
                customer = await _catalogService.GetCustomerByIdAsync(sale.CustomerId.Value);
                if (customer == null)
                    throw new InvalidOperationException("Customer not found.");

            }

            var items = new List<FiscalDocumentItemDTO>();
            decimal netAmount = 0;
            decimal vatAmount = 0;

            // recorrer articles de sales. luego recorrer articles de deliveryNotes y generar FiscalDocumentItemDTO
            foreach (var article in sale.Articles)
            {

                var articleInfo = await _catalogService.GetArticleByIdAsync(article.ArticleId);
                if (articleInfo == null)
                    throw new InvalidOperationException($"Article {article.ArticleId} not found.");

    

                // Replace with this corrected code:
                var deliveryNoteArticles = deliveryNotes
                    .SelectMany(dn => dn.Articles)
                    .Where(a => a.ArticleId == article.ArticleId)
                    .ToList();

                if (deliveryNoteArticles.Count == 0)
                {
                    throw new InvalidOperationException($"No delivery note articles found for article {article.ArticleId}.");
                }

                var totalDeliveredQuantity = deliveryNoteArticles.Sum(a => a.Quantity);
                if (totalDeliveredQuantity < article.Quantity)
                {
                    throw new InvalidOperationException($"Delivery note article {article.ArticleId} has insufficient quantity.");
                }

                foreach(var i in deliveryNoteArticles)
                {
                    var priceWithDiscount = article.UnitPrice * (1 - article.DiscountPercent / 100) * sale.ExchangeRate;
                    var baseAmount = priceWithDiscount * i.Quantity;
                    var iva = baseAmount * (article.IVAPercent / 100);

                    items.Add(new FiscalDocumentItemDTO
                    {
                        Description = articleInfo.Description,
                        UnitPrice = article.UnitPrice,
                        Quantity = (int)article.Quantity,
                        VatBase = baseAmount,
                        VatAmount = iva,
                        VatId = (int)(article.IVAPercent == 21 ? 5 : 4), // Dummy ejemplo
                        DispatchCode = i.DispatchCode

                    });

                    netAmount += baseAmount;
                    vatAmount += iva;
                }



            }
            

            int buyerDocumentType = 0;
            switch (sale.CustomerIdType)
            {
                case "CUIT":
                    buyerDocumentType = 80; // CUIT
                    break;
                case "DNI":
                    buyerDocumentType = 96; // DNI
                    break;
                case "CUIL":
                    buyerDocumentType = 86; // CUIL
                    break;
                default:
                    throw new InvalidOperationException("Invalid customer ID type.");
            }

            var invoiceType = 0;

            if (sale.IsFinalConsumer || customer.IVAType == "Exento" || customer.IVAType == "Monotributo")
                invoiceType = 6; // Factura B
            else 
                invoiceType = 1; // Factura A

            var fiscalRequest = new FiscalDocumentCreateDTO
            {
                PointOfSale = 1,
                InvoiceType = invoiceType,
                BuyerDocumentType = buyerDocumentType,
                BuyerDocumentNumber = long.Parse(sale.CustomerTaxId),
                NetAmount = Math.Round(netAmount, 2),
                VatAmount = Math.Round(vatAmount, 2),
                TotalAmount = Math.Round(netAmount + vatAmount, 2),
                SalesOrderId = saleId,
                Items = items
            };

            var result = await _fiscalServiceClient.CreateInvoiceAsync(fiscalRequest);

            sale.HasInvoice = true;
            _unitOfWork.SaleRepository.Update(sale);
            await _unitOfWork.SaveAsync();

            return MapToBasic(result);
        }
        public async Task<InvoiceBasicDTO> GetInvoiceAsync(int saleId)
        {
            var sale = await _unitOfWork.SaleRepository.GetIncludingAsync(saleId, s => s.Articles);
            if (sale == null)
                throw new InvalidOperationException("Sale not found.");
            if (!sale.HasInvoice)
                throw new InvalidOperationException("No invoice generated for this sale.");
            var invoice = await _fiscalServiceClient.GetBySaleIdAsync(saleId);
            if (invoice == null)
                throw new InvalidOperationException("Invoice not found.");
            return MapToBasic(invoice);
        }
        public async Task<InvoiceDetailDTO> GetInvoiceDetailAsync(int saleId)
        {
            var sale = await _unitOfWork.SaleRepository.GetIncludingAsync(saleId, s => s.Articles);
            if (sale == null)
                throw new InvalidOperationException("Sale not found.");
            if (!sale.HasInvoice)
                throw new InvalidOperationException("No invoice generated for this sale.");
            var invoice = await _fiscalServiceClient.GetBySaleIdAsync(saleId);
            if (invoice == null)
                throw new InvalidOperationException("Invoice not found.");

            // get company info from CompanyInfo Service
            var company = await _companyServiceClient.GetCompanyInfoAsync();

            // get customer info from catalog if not final consumer
            CustomerDTO customer = new CustomerDTO
            {
                Id = 0,
                CompanyName = "Final Consumer",
                TaxId = "N/A",
                Address = "N/A",
                PostalCode = "N/A",
                City = "N/A",
                Province = "N/A",
                Country = "N/A",
                SellCondition = "N/A",
                IVAType = "N/A"
            };
            if (!sale.IsFinalConsumer && sale.CustomerId.HasValue)
            {
                customer = await _catalogService.GetCustomerByIdAsync(sale.CustomerId.Value);
            }
            else
            {
                var postalCode = await _catalogService.GetPostalCodeByIdAsync(sale.CustomerPostalCodeId.Value);

                customer.PostalCode = postalCode?.Code ?? "N/A";
                customer.Province = postalCode?.Province ?? "N/A";
                customer.Country = postalCode?.Country ?? "N/A";
                customer.City = postalCode?.City ?? "N/A";
                customer.CompanyName = sale.CustomerName ?? "Final Consumer";
                customer.IVAType = "Consumidor Final";
                customer.TaxId = sale.CustomerTaxId ?? "N/A";
                customer.SellCondition = "Contado";
            }

            // get seller info from user service
            var seller = await _userService.GetUserByIdAsync(sale.SellerId);

            // generate invoice detail
            var items = invoice.Items.Select(i => new InvoiceDetailItemDTO
            {
                Description = i.Description,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                VatBase = i.VatBase,
                VatAmount = i.VatAmount,
                VatId = i.VatId,
                DispatchCode = i.DispatchCode
            }).ToList();

            return new InvoiceDetailDTO
            {
                Id = invoice.Id,
                DocumentNumber = invoice.DocumentNumber,
                InvoiceType = invoice.InvoiceType,
                PointOfSale = invoice.PointOfSale,
                Date = invoice.Date,
                Cae = invoice.Cae,
                CaeExpiration = invoice.CaeExpiration,
                NetAmount = invoice.NetAmount,
                VatAmount = invoice.VatAmount,
                TotalAmount = invoice.TotalAmount,
                CompanyName = company.Name,
                CompanyShortName = company.ShortName,
                CompanyTaxId = company.TaxId,
                CompanyAddress = company.Address,
                CompanyPostalCode = company.PostalCode,
                CompanyCity = company.City,
                CompanyProvince = company.Province,
                CompanyCountry = company.Country,
                CompanyPhone = company.Phone,
                CompanyEmail = company.Email ?? string.Empty,
                CompanyLogoUrl = company.LogoUrl ?? string.Empty,
                CompanyIVAType = company.IVAType ?? "N/A",
                CompanyGrossIncome = company.GrossIncome ?? string.Empty,
                CompanyDateOfIncorporation = company.DateOfIncorporation,
                CustomerId = customer.Id,
                CustomerName = customer.CompanyName,
                CustomerTaxID = customer.TaxId,
                CustomerAddress = customer.Address,
                CustomerPostalCode = customer.PostalCode,
                CustomerCity = customer.City,
                CustomerProvince = customer.Province,
                CustomerCountry = customer.Country,
                CustomerSellCondition = customer.SellCondition ?? "N/A",
                CustomerIVAType = customer.IVAType ?? "N/A",
                SellerId = sale.SellerId,
                SellerName = $"{seller.FirstName} {seller.LastName}",
                Items = items
            };

        }
        private static InvoiceBasicDTO MapToBasic(FiscalDocumentResponseDTO f) => new()
        {
            Id = f.Id,
            DocumentNumber = f.DocumentNumber,
            InvoiceType = f.InvoiceType,
            PointOfSale = f.PointOfSale,
            Date = f.Date,
            Cae = f.Cae,
            CaeExpiration = f.CaeExpiration,
            NetAmount = f.NetAmount,
            VatAmount = f.VatAmount,
            TotalAmount = f.TotalAmount
        };     
        private async Task ValidateSellerAsync(int sellerId)
        {
            var user = await _userService.GetUserByIdAsync(sellerId);
            if (user == null) throw new ArgumentException("Invalid seller ID.");
        }
        private async Task ValidateCustomerBlockAsync(SaleCreateDTO dto)
        {
            if (!dto.IsFinalConsumer)
            {
                var customer = await _catalogService.GetCustomerByIdAsync(dto.CustomerId!.Value);
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

            var postal = await _catalogService.GetPostalCodeByIdAsync(dto.CustomerPostalCodeId.Value);
            if (postal == null) throw new ArgumentException("Invalid postal code ID.");

            if (dto.CustomerIdType != "DNI" && dto.CustomerIdType != "CUIT" && dto.CustomerIdType != "CUIL")
                throw new ArgumentException("Customer ID type must be DNI, CUIT or CUIL for final consumer sales.");

            if (string.IsNullOrWhiteSpace(dto.CustomerTaxId))
                throw new ArgumentException("Customer tax ID is required for final consumer sales.");
        }
        private async Task ValidateArticlesAndStockAsync(SaleCreateDTO dto)
        {
            foreach (var a in dto.Articles)
            {
                var art = await _catalogService.GetArticleByIdAsync(a.ArticleId);
                if (art == null) throw new ArgumentException($"Invalid article ID: {a.ArticleId}");
                if (a.Quantity <= 0) throw new ArgumentException($"Invalid quantity for article {a.ArticleId}.");

                // Si tu StockService soporta chequeo por depósito, usá esa sobrecarga.
                var available = await _stockServiceClient.GetAvailableStockAsync(a.ArticleId);
                if (available < a.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for article {a.ArticleId}. Available: {available}, Requested: {a.Quantity}.");
            }
        }

    }
}
