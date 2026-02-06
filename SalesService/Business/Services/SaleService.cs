using SalesService.Business.Interfaces;
using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.Clients;
using SalesService.Business.Models.DeliveryNote;
using SalesService.Business.Models.Sale;
using SalesService.Business.Models.Sale.accounting;
using SalesService.Business.Models.Sale.fiscalDocs;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.Sale;
using System.Text.RegularExpressions;


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
        private readonly IAccountingServiceClient _accountingServiceClient;

        public SaleService(
            IUnitOfWork unitOfWork,
            ICatalogServiceClient catalogService,
            IStockServiceClient stockServiceClient,
            IUserServiceClient userService,
            IFiscalServiceClient fiscalServiceClient,
            IAccountingServiceClient accountingServiceClient,
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
            _accountingServiceClient = accountingServiceClient;
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
                    IsFullyDelivered = sale.IsFullyDelivered,
                    HasStockWarning = sale.HasStockWarning
                });
            }

            return result;
        }
        public async Task<SaleDetailDTO?> GetByIdAsync(int id)
        {
            var sale = await _unitOfWork.SaleRepository.GetIncludingAsync(id, s => s.Articles, s => s.StockWarnings);
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


            var warningDTOs = new List<SaleStockWarningDTO>();

            foreach (var warning in sale.StockWarnings)
            {
                warningDTOs.Add(new SaleStockWarningDTO
                {
                    ArticleId = warning.ArticleId,
                    ShortageSnapshot = warning.ShortageSnapshot,
                    IsResolved = warning.IsResolved,
                    ResolvedAt = warning.ResolvedAt 
                });
            }

            return new SaleDetailDTO
            {
                Id = sale.Id,
                Date = sale.Date,
                ExchangeRate = sale.ExchangeRate,
                Observations = sale.Observations,
                IsFinalConsumer = sale.IsFinalConsumer,
                HasStockWarning = sale.HasStockWarning,

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
                Warnings = warningDTOs,
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

            if (!dto.IsCash && dto.IsFinalConsumer)
                throw new InvalidOperationException("Non-cash sales cannot be made to final consumers.");

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


                // 4) Emitir Factura ( y recibo si es al contado)
                var invoice = await CreateInvoiceAsync(sale.Id, dto.IsCash);


                // 5) Marcar flags (por si tu lógica interna no lo hizo ya)
                sale.HasInvoice = true;

                _unitOfWork.SaleRepository.Update(sale);
                await _unitOfWork.SaveAsync();

                await tx.CommitAsync();

                // 6) Devolver todo junto
                var saleDetail = await GetByIdAsync(sale.Id);
                // Agregar advice de imputación si corresponde
                invoice.AllocationAdvice = await GetAllocationAdviceAsync(sale.Id, invoice, CancellationToken.None);

                return new QuickSaleResultDTO
                {
                    Sale = saleDetail!,
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
        public async Task<InvoiceBasicDTO> CreateInvoiceAsync(int saleId, bool IsCash)
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

            if (!IsCash && sale.IsFinalConsumer)
                throw new InvalidOperationException("Non-cash sales cannot be invoiced to final consumers.");

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
                IVAType = "N/A", 
                IVATypeArcaCode= 5
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

                foreach (var i in deliveryNoteArticles)
                {
                    var priceWithDiscount = article.UnitPrice * (1 - article.DiscountPercent / 100) * sale.ExchangeRate;
                    var baseAmount = priceWithDiscount * i.Quantity;
                    var iva = baseAmount * (article.IVAPercent / 100);

                    items.Add(new FiscalDocumentItemDTO
                    {
                        Sku = articleInfo.SKU,
                        Description = articleInfo.Description + "- Marca: " + articleInfo.Brand,
                        UnitPrice = priceWithDiscount,
                        Quantity = (int)i.Quantity,
                        VatBase = baseAmount,
                        VatAmount = iva,
                        VatId = (int)(article.IVAPercent ==21 ?5 :4), // Dummy ejemplo
                        DispatchCode = i.DispatchCode,
                        Warranty = articleInfo.Warranty

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
                InvoiceType = invoiceType,
                BuyerDocumentType = buyerDocumentType,
                BuyerDocumentNumber = long.Parse(sale.CustomerTaxId),
                ReceiverVatConditionId = customer.IVATypeArcaCode,
                NetAmount = Math.Round(netAmount, 2),
                VatAmount = Math.Round(vatAmount, 2),
                TotalAmount = Math.Round(netAmount + vatAmount, 2),
                SalesOrderId = saleId,
                Items = items,
                Currency = "PES", // Default currency
                ExchangeRate = 1
            };

            var result = await _fiscalServiceClient.CreateFiscalNoteAsync(fiscalRequest);


            // ---> Notificar a Accounting (solo clientes; no consumidor final)
            if (!sale.IsFinalConsumer && sale.CustomerId.HasValue)
            {
                try
                {
                    await _accountingServiceClient.UpsertExternalAsync(new AccountingExternalUpsertDTO
                    {
                        PartyType = "customer",                                  // Customer
                        PartyId = sale.CustomerId.Value,
                        Kind = "invoice",                                       // Invoice
                        ExternalRefId = result.Id,
                        ExternalRefNumber = result.DocumentNumber,
                        DocumentDate = result.Date,
                        Currency = "ARS",
                        FxRate = 1,
                        AmountARS = result.TotalAmount,
                        IsCash = IsCash
                    });

                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Error notifying accounting.", ex);
                }
            }


            sale.HasInvoice = true;
            _unitOfWork.SaleRepository.Update(sale);
            await _unitOfWork.SaveAsync();

            // Agregar advice de imputación
            var invoice = MapToBasic(result);
            invoice.AllocationAdvice = await GetAllocationAdviceAsync(saleId, invoice, CancellationToken.None);
            return invoice;
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
        public async Task<InvoiceDetailDTO> GetInvoiceDetailAsync(int Id)
        {

            var invoice = await _fiscalServiceClient.GetByIdAsync(Id);
            if (invoice == null)
                throw new InvalidOperationException("Invoice not found.");
            var sale = await _unitOfWork.SaleRepository.GetByIdAsync(invoice.SalesOrderId.Value);
            if (sale == null)
                throw new InvalidOperationException("Sale not found.");

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
                Sku = i.Sku,
                Description = i.Description,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                VatBase = i.VatBase,
                VatAmount = i.VatAmount,
                VatId = i.VatId,
                VatPercentage = (i.VatId == 5) ? 21 : 10.5m, // Dummy ejemplo
                DispatchCode = i.DispatchCode,
                Warranty = i.Warranty
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
                ArcaQrUrl = invoice.ArcaQrUrl ?? string.Empty,
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
        public async Task<InvoiceBasicDTO> CreateCreditNoteAsync(int saleId, CreditNoteCreateForSaleDTO dto)
        {
            var sale = await _unitOfWork.SaleRepository.GetIncludingAsync(saleId, s => s.Articles)
                       ?? throw new InvalidOperationException("Sale not found.");

            if (!sale.HasInvoice)
                throw new InvalidOperationException("Cannot create a credit note without an invoice.");

            // 1) Factura base de esta venta (para referenciar)
            var baseInvoice = await _fiscalServiceClient.GetBySaleIdAsync(sale.Id)
                             ?? throw new InvalidOperationException("Base invoice not found.");

            // 1) Traer NC previas de la factura base
            var previousCNs = await _fiscalServiceClient.GetCreditNotesAsync(sale.Id);

            // 2) Sumar total acumulado de NC previas (para validar contra el total de la factura)
            var previousCNsTotal = previousCNs.Sum(n => n.TotalAmount);


            // define invoice type based on customer
            var invoiceType = 0;
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
                IVAType = "N/A",
                IVATypeArcaCode = 5
            };

            if (!sale.IsFinalConsumer)
            {
                customer = await _catalogService.GetCustomerByIdAsync(sale.CustomerId.Value);
                if (customer == null)
                    throw new InvalidOperationException("Customer not found.");

            }


            if (sale.IsFinalConsumer || customer.IVAType == "Exento" || customer.IVAType == "Monotributo")
                // Nota Credito tipo b
                invoiceType = 8; // Nota de Crédito B

            else
                invoiceType = 3; // Nota de Crédito A


            // 3) Si el motivo es devolución por ítems, validar cantidades acumuladas por artículo
            if (dto.Reason == CreditNoteReason.PartialReturn && dto.Items != null && dto.Items.Count > 0)
            {
                // Mapa SKU -> devuelto acumulado (desde NC previas)
                var returnedQtyBySku = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

                foreach (var note in previousCNs)
                {
                    if (note.Items == null) continue;
                    foreach (var it in note.Items)
                    {
                        // ignorar líneas genéricas de ajuste monetario (si las hubo)
                        if (string.IsNullOrWhiteSpace(it.Sku) || it.Sku == "ADJ-CREDIT") continue;

                        if (!returnedQtyBySku.ContainsKey(it.Sku))
                            returnedQtyBySku[it.Sku] = 0;

                        returnedQtyBySku[it.Sku] += it.Quantity;
                    }
                }

                // Validar que lo solicitado no exceda lo vendido por artículo
                foreach (var req in dto.Items)
                {
                    var saleLine = sale.Articles.FirstOrDefault(a => a.ArticleId == req.ArticleId)
                                   ?? throw new InvalidOperationException($"Article {req.ArticleId} not found in sale.");

                    var artInfo = await _catalogService.GetArticleByIdAsync(req.ArticleId)
                                 ?? throw new InvalidOperationException($"Article {req.ArticleId} not found in catalog.");

                    var prevReturned = returnedQtyBySku.TryGetValue(artInfo.SKU, out var q) ? q : 0m;
                    var requested = req.Quantity;
                    var sold = saleLine.Quantity;

                    if (requested <= 0)
                        throw new InvalidOperationException($"Invalid quantity for article {req.ArticleId}.");

                    if (prevReturned + requested > sold)
                    {
                        throw new InvalidOperationException(
                            $"Accumulated return for article {req.ArticleId} exceeds sold quantity. " +
                            $"Sold: {sold}, Returned: {prevReturned}, Requested: {requested}.");
                    }
                }
            }

            // 2) Buyer (mismas reglas que al facturar)
            int buyerDocumentType = sale.CustomerIdType switch
            {
                "CUIT" => 80,
                "DNI" => 96,
                "CUIL" => 86,
                _ => throw new InvalidOperationException("Invalid customer ID type.")
            };

            if (string.IsNullOrWhiteSpace(sale.CustomerTaxId))
                throw new InvalidOperationException("Customer TaxId is required.");


            // 4) Validación por motivo + armado de Items/Importes
            var items = new List<FiscalDocumentItemDTO>();
            decimal netAmount = 0m, vatAmount = 0m;

            if (dto.Reason == CreditNoteReason.PartialReturn)
            {
                // --- NC por ÍTEMS ---
                if (dto.Items == null || dto.Items.Count == 0)
                    throw new InvalidOperationException("Items are required for PartialReturn.");

                foreach (var req in dto.Items)
                {
                    if (req.Quantity <= 0) throw new InvalidOperationException("Quantity must be > 0.");

                    var saleLine = sale.Articles.FirstOrDefault(a => a.ArticleId == req.ArticleId)
                                   ?? throw new InvalidOperationException($"Article {req.ArticleId} not found in sale.");

                    // TODO (opcional): validar que no exceda lo ya devuelto acumulado

                    var articleInfo = await _catalogService.GetArticleByIdAsync(req.ArticleId)
                                     ?? throw new InvalidOperationException($"Article {req.ArticleId} not found in catalog.");

                    var priceWithDiscount = saleLine.UnitPrice * (1 - saleLine.DiscountPercent / 100m) * sale.ExchangeRate;
                    var baseLine = priceWithDiscount * req.Quantity;
                    var lineVat = Math.Round(baseLine * (saleLine.IVAPercent / 100m), 2);

                    items.Add(new FiscalDocumentItemDTO
                    {
                        Sku = articleInfo.SKU,
                        Description = $"{articleInfo.Description} - Marca: {articleInfo.Brand}",
                        UnitPrice = Math.Round(priceWithDiscount, 2),
                        Quantity = (int)req.Quantity,
                        VatBase = Math.Round(baseLine, 2),
                        VatAmount = lineVat,
                        VatId = saleLine.IVAPercent == 21 ? 5 : 4, // mismo criterior que usas para facturas
                        DispatchCode = null,
                        Warranty = articleInfo.Warranty
                    });

                    netAmount += Math.Round(baseLine, 2);
                    vatAmount += lineVat;
                }
            }
            else
            {
                // --- NC por MONTO ---
                if (!dto.NetAmount.HasValue || dto.NetAmount.Value <= 0)
                    throw new InvalidOperationException("NetAmount must be provided and > 0.");

                if (dto.VatAmount.HasValue)
                {
                    netAmount = Math.Round(dto.NetAmount.Value, 2);
                    vatAmount = Math.Round(dto.VatAmount.Value, 2);
                }
                else
                {
                    if (!dto.VatPercent.HasValue || dto.VatPercent.Value < 0)
                        throw new InvalidOperationException("VatPercent must be provided when VatAmount is not present.");

                    netAmount = Math.Round(dto.NetAmount.Value, 2);
                    vatAmount = Math.Round(netAmount * (dto.VatPercent.Value / 100m), 2);
                }

                // Podés enviar 1 ítem genérico si Fiscal lo prefiere, o dejar Items vacío.
                items.Add(new FiscalDocumentItemDTO
                {
                    Sku = "ADJ-CREDIT",
                    Description = dto.Comment ?? "Ajuste de Credito",
                    UnitPrice = netAmount,
                    Quantity = 1,
                    VatBase = netAmount,
                    VatAmount = vatAmount,
                    VatId = (dto.VatPercent ?? 21m) == 21m ? 5 : 4,
                    DispatchCode = null,
                    Warranty = 0
                });
            }

            var totalAmount = Math.Round(netAmount + vatAmount, 2);

            var newCNTot = Math.Round(netAmount + vatAmount, 2);
            if (previousCNsTotal + newCNTot > baseInvoice.TotalAmount)
            {
                throw new InvalidOperationException(
                    $"Credit notes total would exceed the base invoice total. " +
                    $"Invoice total: {baseInvoice.TotalAmount}, " +
                    $"Already credited: {previousCNsTotal}, " +
                    $"This CN: {newCNTot}.");
            }


            //// 5) Armar request al servicio Fiscal
            ///
            //convert baseInvoice.DocumentNumber to long


            var request = new FiscalDocumentCreateDTO
            {

                InvoiceType = invoiceType,
                BuyerDocumentType = buyerDocumentType,
                BuyerDocumentNumber = long.Parse(sale.CustomerTaxId),
                ReceiverVatConditionId = customer.IVATypeArcaCode,
                NetAmount = netAmount,
                VatAmount = vatAmount,
                TotalAmount = totalAmount,
                SalesOrderId = saleId,
                Items = items,
                Currency = "PES", // Default currency
                ExchangeRate = 1,

                ReferencedInvoiceType = baseInvoice.InvoiceType,
                ReferencedPointOfSale = baseInvoice.PointOfSale,
                ReferencedInvoiceNumber = ConvertDocumentNumberRightToLong(baseInvoice.DocumentNumber)
            };


            var created = await _fiscalServiceClient.CreateFiscalNoteAsync(request);

            if (!sale.IsFinalConsumer && sale.CustomerId.HasValue)
            {
                try
                {
                    await _accountingServiceClient.UpsertExternalAsync(new AccountingExternalUpsertDTO
                    {
                        PartyType = "customer",
                        PartyId = sale.CustomerId.Value,
                        Kind = "creditNote",                                       // CreditNote
                        ExternalRefId = created.Id,
                        ExternalRefNumber = created.DocumentNumber,
                        DocumentDate = created.Date,
                        Currency = "ARS",
                        FxRate = 1m,
                        AmountARS = created.TotalAmount
                    });

                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Error notifying accounting.", ex);
                }
            }


            // si es devolución física, registrar ingreso de mercadería
            if (dto.Reason == CreditNoteReason.PartialReturn)
            {
                if (!dto.ReturnWarehouseId.HasValue || dto.ReturnWarehouseId.Value <= 0)
                    throw new InvalidOperationException("ReturnWarehouseId is required for PartialReturn.");

                // por cada ítem devuelto, un movimiento hacia el depósito destino
                foreach (var it in dto.Items!)
                {
                    // validación simple
                    if (it.Quantity <= 0)
                        throw new InvalidOperationException("Item quantity must be > 0 for PartialReturn.");

                    var movement = new StockMovementCreateDTO
                    {
                        ArticleId = it.ArticleId,
                        FromWarehouseId = null,                                            // viene de cliente → no hay depósito de origen
                        ToWarehouseId = dto.ReturnWarehouseId.Value,                       // depósito que recibe
                        Quantity = it.Quantity,                                            // cantidad devuelta
                        Reference = $"Nota de Credito {created.DocumentNumber}",           // referencia útil (ej: "CN 0001-00001234")
                        DispatchId = null,                                                 // si querés enlazar a un remito, cargarlo acá
                        MovementType = 2                                                   // ⬅️ nuestro tipo “Adjustment”
                    };

                    await _stockServiceClient.RegisterQuickStockMovementAsync(movement);
                }
            }

            // 6) Respuesta en tu DTO resumido (mismo que usas para la factura)
            return MapToBasic(created);
        }
        public async Task<InvoiceBasicDTO> CreateDebitNoteAsync(int saleId, DebitNoteCreateForSaleDTO dto)
        {
            var sale = await _unitOfWork.SaleRepository.GetIncludingAsync(saleId, s => s.Articles)
                       ?? throw new InvalidOperationException("Sale not found.");

            if (!sale.HasInvoice)
                throw new InvalidOperationException("Cannot create a debit note without an invoice.");

            // 1) Factura base de esta venta (para referenciar)
            var baseInvoice = await _fiscalServiceClient.GetBySaleIdAsync(sale.Id)
                             ?? throw new InvalidOperationException("Base invoice not found.");

            // 2) Buyer (mismas reglas que al facturar)
            int buyerDocumentType = sale.CustomerIdType switch
            {
                "CUIT" => 80,
                "DNI" => 96,
                "CUIL" => 86,
                _ => throw new InvalidOperationException("Invalid customer ID type.")
            };

            if (string.IsNullOrWhiteSpace(sale.CustomerTaxId))
                throw new InvalidOperationException("Customer TaxId is required.");

            // define invoice type based on customer
            var invoiceType = 0;
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
                IVAType = "N/A",
                IVATypeArcaCode = 5
            };

            if (!sale.IsFinalConsumer)
            {
                customer = await _catalogService.GetCustomerByIdAsync(sale.CustomerId.Value);
                if (customer == null)
                    throw new InvalidOperationException("Customer not found.");

            }


            if (sale.IsFinalConsumer || customer.IVAType == "Exento" || customer.IVAType == "Monotributo")
                // Nota Credito tipo b
                invoiceType = 7; // Nota de Debito B

            else
                invoiceType = 2; // Nota de Debito A


            // 4) Validaciones + cálculo de importes
            if (dto.NetAmount <= 0)
                throw new InvalidOperationException("NetAmount must be > 0.");

            decimal netAmount = Math.Round(dto.NetAmount, 2);
            decimal vatAmount;

            if (dto.VatAmount.HasValue)
            {
                vatAmount = Math.Round(dto.VatAmount.Value, 2);
            }
            else
            {
                if (!dto.VatPercent.HasValue || dto.VatPercent.Value < 0)
                    throw new InvalidOperationException("VatPercent must be provided when VatAmount is not present.");
                vatAmount = Math.Round(netAmount * (dto.VatPercent.Value / 100m), 2);
            }

            var totalAmount = Math.Round(netAmount + vatAmount, 2);


            // crear item generico
            var items = new List<FiscalDocumentItemDTO>
            {
                new FiscalDocumentItemDTO
                {
                    Sku = "ADJ-DEBIT",
                    Description = dto.Comment ?? "Ajuste de Debito",
                    UnitPrice = netAmount,
                    Quantity = 1,
                    VatBase = netAmount,
                    VatAmount = vatAmount,
                    VatId = (dto.VatPercent ?? 21m) == 21m ? 5 : 4,
                    DispatchCode = null,
                    Warranty = 0
                }
            };


            // 5) Armar request al servicio Fiscal
            var request = new FiscalDocumentCreateDTO
            {
                InvoiceType = invoiceType,
                BuyerDocumentType = buyerDocumentType,
                BuyerDocumentNumber = long.Parse(sale.CustomerTaxId),
                ReceiverVatConditionId = customer.IVATypeArcaCode,
                NetAmount = netAmount,
                VatAmount = vatAmount,
                TotalAmount = totalAmount,
                SalesOrderId = saleId,
                Items = items,
                Currency = "PES", // Default currency
                ExchangeRate = 1,

                ReferencedInvoiceType = baseInvoice.InvoiceType,
                ReferencedPointOfSale = baseInvoice.PointOfSale,
                ReferencedInvoiceNumber = ConvertDocumentNumberRightToLong(baseInvoice.DocumentNumber)
            };

            var created = await _fiscalServiceClient.CreateFiscalNoteAsync(request);

            if (!sale.IsFinalConsumer && sale.CustomerId.HasValue)
            {
                try
                {
                    await _accountingServiceClient.UpsertExternalAsync(new AccountingExternalUpsertDTO
                    {
                        PartyType = "customer",
                        PartyId = sale.CustomerId.Value,
                        Kind = "debitNote",                                       // DebitNote
                        ExternalRefId = created.Id,
                        ExternalRefNumber = created.DocumentNumber,
                        DocumentDate = created.Date,
                        Currency = "ARS",
                        FxRate = 1m,
                        AmountARS = created.TotalAmount
                    });

                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Error notifying accounting.", ex);
                }
            }


            // 6) Devolver en formato básico (mismo que usás para factura/NC)
            var invoice = MapToBasic(created);
            invoice.AllocationAdvice = await GetAllocationAdviceAsync(saleId, invoice, CancellationToken.None);
            return invoice;
        }
        public async Task<IReadOnlyList<SaleNoteSummaryDTO>> GetCreditNotesAsync(int saleId)
        {
            var baseInvoice = await _fiscalServiceClient.GetBySaleIdAsync(saleId)
                             ?? throw new InvalidOperationException("Base invoice not found.");
            var notes = await _fiscalServiceClient.GetCreditNotesAsync(saleId);

            return notes.Select(n => new SaleNoteSummaryDTO
            {
                Id = n.Id,
                DocumentNumber = n.DocumentNumber,
                Date = n.Date,
                TotalAmount = n.TotalAmount,
                Kind = "CreditNote"
            }).OrderByDescending(x => x.Date).ToList();
        }
        public async Task<IReadOnlyList<SaleNoteSummaryDTO>> GetDebitNotesAsync(int saleId)
        {
            var baseInvoice = await _fiscalServiceClient.GetBySaleIdAsync(saleId)
                             ?? throw new InvalidOperationException("Base invoice not found.");
            var notes = await _fiscalServiceClient.GetDebitNotesAsync(saleId);

            return notes.Select(n => new SaleNoteSummaryDTO
            {
                Id = n.Id,
                DocumentNumber = n.DocumentNumber,
                Date = n.Date,
                TotalAmount = n.TotalAmount,
                Kind = "DebitNote"
            }).OrderByDescending(x => x.Date).ToList();
        }
        public async Task<IReadOnlyList<SaleNoteSummaryDTO>> GetAllNotesAsync(int saleId)
        {
            var credit = await GetCreditNotesAsync(saleId);
            var debit = await GetDebitNotesAsync(saleId);
            return credit.Concat(debit).OrderByDescending(x => x.Date).ToList();
        }
        public async Task CoverInvoiceWithReceiptsAsync(int saleId, CoverInvoiceRequest request, CancellationToken ct = default)
        {
            var sale = await _unitOfWork.SaleRepository.GetByIdAsync(saleId);
            if (sale is null) throw new KeyNotFoundException($"Sale {saleId} not found.");

            if (!sale.CustomerId.HasValue)
                throw new InvalidOperationException("Sale has no CustomerId (final consumer).");

            if (request.PartyId != sale.CustomerId.Value)
                throw new InvalidOperationException("Cover request party does not match sale customer.");
            
            request.PartyType = "customer";

            await _accountingServiceClient.CoverInvoiceWithReceiptsAsync(request, ct);
        }
        public async Task RegisterStockWarningsAsync(IEnumerable<SaleStockWarningInputDTO> warnings)
        {
            if (warnings == null) return;

            var list = warnings.ToList();
            if (!list.Any()) return;

            var now = DateTime.UtcNow;

            // Group by sale so we can update header flags once per sale
            var warningsBySale = list.GroupBy(w => w.SaleId);

            foreach (var group in warningsBySale)
            {
                var saleId = group.Key;

                var sale = await _unitOfWork.SaleRepository.GetByIdAsync(saleId);
                if (sale == null)
                {
                    // If sale does not exist (old id, deleted, etc.), we skip it.
                    continue;
                }

                // Mark sale as having stock warnings and update timestamp
                sale.HasStockWarning = true;
                sale.StockWarningUpdatedAt = now;
                _unitOfWork.SaleRepository.Update(sale);

                foreach (var warning in group)
                {
                    // Check if there is already an active warning for this sale/article
                    var existing = await _unitOfWork.SaleStockWarningRepository
                        .GetActiveBySaleAndArticleAsync(saleId, warning.ArticleId);

                    if (existing != null)
                    {
                        // Update snapshot and timestamp for an existing warning
                        existing.ShortageSnapshot = warning.ShortageSnapshot;
                        existing.CreatedAt = now;
                    }
                    else
                    {
                        // Create a new warning entry
                        var newWarning = new SaleStockWarning
                        {
                            SaleId = saleId,
                            ArticleId = warning.ArticleId,
                            ShortageSnapshot = warning.ShortageSnapshot,
                            IsResolved = false,
                            CreatedAt = now
                        };

                        await _unitOfWork.SaleStockWarningRepository.AddAsync(newWarning);
                    }
                }
            }

            await _unitOfWork.SaveAsync();
        }
        public async Task<SaleResolveStockWarningResultDTO> ResolveStockWarningAsync(int saleId, SaleResolveStockWarningDTO dto, int userId)
        {
            if (dto.Articles == null || dto.Articles.Count == 0)
                throw new ArgumentException("No article modifications were provided.");

            // Traemos venta con artículos + warnings
            var sale = await _unitOfWork.SaleRepository
                .GetIncludingAsync(saleId, s => s.Articles, s => s.StockWarnings);

            if (sale == null)
                throw new KeyNotFoundException($"Sale {saleId} not found.");

            if (!sale.HasStockWarning)
                throw new InvalidOperationException("Sale has no active stock warnings.");

            // Warnings activos de ESTA venta agrupados por artículo
            var warningsByArticle = sale.StockWarnings
                .Where(w => !w.IsResolved)
                .GroupBy(w => w.ArticleId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Artículos actuales de la venta
            var saleArticles = sale.Articles.ToDictionary(a => a.ArticleId, a => a);

            // Snapshot de cantidades originales ANTES de modificar nada
            var originalQuantities = sale.Articles
                .ToDictionary(a => a.ArticleId, a => a.Quantity);

            using var tx = await _unitOfWork.SaleRepository.BeginTransactionAsync();
            try
            {
                // 1) función helper: cuánto reduce ESTA venta por artículo
                decimal TotalReductionForArticle(int articleId)
                {
                    decimal reduction = 0m;

                    foreach (var item in dto.Articles.Where(x => x.ArticleId == articleId))
                    {
                        if (!originalQuantities.TryGetValue(item.ArticleId, out var originalQty))
                            throw new Exception($"Article {item.ArticleId} not found in sale {saleId}.");

                        if (item.Quantity > originalQty)
                            throw new Exception($"Quantity cannot be increased for article {item.ArticleId}.");

                        reduction += (originalQty - item.Quantity);
                    }

                    return reduction;
                }

                // 2) Aplicar modificaciones de cantidades
                foreach (var mod in dto.Articles)
                {
                    var line = saleArticles[mod.ArticleId];

                    if (mod.Quantity == 0)
                    {
                        await _unitOfWork.SaleArticleRepository.DeleteAsync(line.Id);
                    }
                    else if (mod.Quantity != line.Quantity)
                    {
                        line.Quantity = mod.Quantity;
                        _unitOfWork.SaleArticleRepository.Update(line);
                    }
                }

                await _unitOfWork.SaveAsync();

                // 3) ¿Quedó la venta sin artículos?
                var remainingLines = await _unitOfWork.SaleArticleRepository
                    .FindAsync(a => a.SaleId == saleId);

                if (!remainingLines.Any())
                {
                    await _unitOfWork.SaleRepository.DeleteAsync(saleId);
                    await _unitOfWork.SaveAsync();
                    await tx.CommitAsync();

                    return new SaleResolveStockWarningResultDTO
                    {
                        IsCancelled = true,
                        Sale = null
                    };
                }

                // 4) Recalcular shortage por artículo y actualizar warnings
                var affectedSaleIds = new HashSet<int> { saleId }; // siempre incluye la venta actual
                foreach (var articleGroup in warningsByArticle)
                {
                    int articleId = articleGroup.Key;

                    // shortage global viejo
                    decimal oldShortage = articleGroup.Value.First().ShortageSnapshot;

                    // reducción en ESTA venta para este artículo
                    decimal reduction = TotalReductionForArticle(articleId);
                    if (reduction <= 0)
                        continue; // no cambió nada para este artículo

                    decimal newShortage = oldShortage - reduction;
                    if (newShortage < 0) newShortage = 0;

                    // Actualizar warnings de este artículo en TODAS las ventas
                    var allWarningsOfArticle = await _unitOfWork.SaleStockWarningRepository
                        .FindAsync(w => w.ArticleId == articleId && !w.IsResolved);

                    foreach (var w in allWarningsOfArticle)
                    {
                        affectedSaleIds.Add(w.SaleId);

                        if (newShortage == 0)
                        {
                            w.IsResolved = true;
                            w.ResolvedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            w.ShortageSnapshot = newShortage;
                        }

                        _unitOfWork.SaleStockWarningRepository.Update(w);
                    }
                }

                await _unitOfWork.SaveAsync();

                // 5) Recalcular HasStockWarning para TODAS las ventas afectadas
                var now = DateTime.UtcNow;
                foreach (var affectedSaleId in affectedSaleIds)
                {
                    // ¿Quedan warnings activos para esta venta?
                    var hasActiveWarnings = (await _unitOfWork.SaleStockWarningRepository
                            .FindAsync(w => w.SaleId == affectedSaleId && !w.IsResolved))
                        .Any();

                    Sale saleToUpdate;
                    if (affectedSaleId == sale.Id)
                    {
                        saleToUpdate = sale;
                    }
                    else
                    {
                        saleToUpdate = await _unitOfWork.SaleRepository.GetByIdAsync(affectedSaleId);
                        if (saleToUpdate == null) continue;
                    }

                    saleToUpdate.HasStockWarning = hasActiveWarnings;
                    saleToUpdate.StockWarningUpdatedAt = now;
                    _unitOfWork.SaleRepository.Update(saleToUpdate);
                }

                await _unitOfWork.SaveAsync();

                // 6) Llamado a StockService (lo tenías ya implementado, lo dejo igual)
                var payload = new CommitedStockEntryUpdateDTO
                {
                    SaleId = saleId,
                    Articles = remainingLines.Select(l => new CommitedStockEntryArticleUpdateDTO
                    {
                        ArticleId = l.ArticleId,
                        NewQuantity = l.Quantity
                    }).ToList()
                };

                await _stockServiceClient.UpdateCommitedStockEntryAsync(payload);

                await tx.CommitAsync();

                // Recargar el detalle para devolverlo actualizado
                var detail = await GetByIdAsync(saleId);

                return new SaleResolveStockWarningResultDTO
                {
                    IsCancelled = false,
                    Sale = detail
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }




        private async Task UpdateStockToCommited(int saleId, List<SaleArticleCreateDTO> articles, int? customerId, string customerName)
        {
            foreach (var article in articles)
            {
                var commitedEntry = new CommitedStockEntryCreateDTO
                {
                    SaleId = saleId,
                    CustomerId = customerId,
                    CustomerName = customerName,
                    ArticleId = article.ArticleId,
                    Quantity = article.Quantity
                };
                await _stockServiceClient.RegisterCommitedStockAsync(commitedEntry);
            }
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
        private async Task<AllocationAdviceDTO?> GetAllocationAdviceAsync(int saleId, InvoiceBasicDTO invoice, CancellationToken ct)
        {
            var sale = await _unitOfWork.SaleRepository.GetByIdAsync(saleId);
            if (sale == null || !sale.HasInvoice)
                return null;

            // Si es CF o no hay cliente, no ofrecemos imputación
            if (!sale.CustomerId.HasValue || sale.IsFinalConsumer)
                return null;

            // Traer recibos con saldo (Accounting)
            var receipts = await _accountingServiceClient.GetReceiptCreditsAsync(sale.CustomerId.Value, ct);
            var totalCredit = receipts.Sum(r => r.PendingARS);


            // Armado
            // Si el total de recibos no cubre la factura, no ofrecemos imputación
            if (totalCredit < invoice.TotalAmount)
                return null;
            return new AllocationAdviceDTO
                {
                CanCoverWithReceipts = true,
                InvoiceExternalRefId = invoice.Id,     // el ExternalRefId que Accounting guardó para la factura
                InvoicePendingARS = invoice.TotalAmount,
                Candidates = receipts.ToList()
            };

        }

        // helper to extract right-hand numeric part of document number like "0001-00001234" -> 1234
        private static long ConvertDocumentNumberRightToLong(string documentNumber)
        {
            if (string.IsNullOrWhiteSpace(documentNumber))
                throw new InvalidOperationException("Invalid referenced document number.");

            // split by '-' and take right-most segment
            var parts = documentNumber.Split('-');
            var right = parts.Length > 1 ? parts[parts.Length - 1] : parts[0];

            // keep only digits
            var digits = Regex.Replace(right, "\\D", "");
            if (string.IsNullOrWhiteSpace(digits) || !long.TryParse(digits, out var result))
                throw new InvalidOperationException($"Invalid referenced document number format: '{documentNumber}'");

            return result;
        }

    }
}
