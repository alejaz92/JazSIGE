using PurchaseService.Business.Exceptions;
using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Models;
using PurchaseService.Infrastructure.Interfaces;
using PurchaseService.Infrastructure.Models;
using PurchaseService.Infrastructure.Repositories;

namespace PurchaseService.Business.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IDispatchRepository _dispatchRepository;
        private readonly IPurchaseDocumentRepository _purchaseDocumentRepository;

        private readonly IPurchaseDocumentService _purchaseDocumentService;
        private readonly ICatalogServiceClient _catalogServiceClient;
        private readonly IUserServiceClient _userServiceClient;
        private readonly IStockServiceClient _stockServiceClient;

        public PurchaseService(
            IPurchaseRepository purchaseRepository,
            IDispatchRepository dispatchRepository,
            IPurchaseDocumentRepository purchaseDocumentRepository,
            IPurchaseDocumentService purchaseDocumentService,
            ICatalogServiceClient catalogServiceClient,
            IUserServiceClient userServiceClient,
            IStockServiceClient stockServiceClient
            )
        {
            _purchaseRepository = purchaseRepository;
            _dispatchRepository = dispatchRepository;
            _purchaseDocumentRepository = purchaseDocumentRepository;
            _purchaseDocumentService = purchaseDocumentService;
            _catalogServiceClient = catalogServiceClient;
            _userServiceClient = userServiceClient;
            _stockServiceClient = stockServiceClient;
        }

        public async Task<IEnumerable<PurchaseDTO>> GetAllAsync()
        {
            var purchases = await _purchaseRepository.GetAllAsync();
            return await MapToDTOListAsync(purchases);
        }
        public async Task<(IEnumerable<PurchaseDTO> purchases, int totalCount)> GetAllAsync(int pageNumber, int pageSize)
        {
            var purchases = await _purchaseRepository.GetAllAsync(pageNumber, pageSize);
            var totalCount = await _purchaseRepository.GetTotalCountAsync();
            var purchaseDTOs = await MapToDTOListAsync(purchases);
            return (purchaseDTOs, totalCount);
        }
        public async Task<PurchaseDTO?> GetByIdAsync(int id)
        {
            var purchase = await _purchaseRepository.GetByIdAsync(id);
            if (purchase == null) return null;

            return await MapToDTOAsync(purchase);
        }
        private async Task UpdateStockConsolidated(int userId, int warehouseId, string reference, int? dispatchId, List<PurchaseArticleCreateDTO> articles)
        {
            // Impactar stock
            try
            {



                await _stockServiceClient.RegisterPurchaseMovementsAsync(
                    userId,
                    warehouseId,
                    articles.Select(a => (a.ArticleId, a.Quantity)).ToList(),
                    reference,
                    dispatchId
                );

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al impactar stock: {ex.Message}");
                throw new PartialSuccessException("La compra fue registrada, pero no se pudo actualizar el stock.");
            }
        }
        private async Task UpdateStockPending(int purchaseId, List<PurchaseArticleCreateDTO> articles)
        {
            foreach (var item in articles)
            {
                var pendingDto = new PendingStockEntryCreateDTO
                {
                    PurchaseId = purchaseId,
                    ArticleId = item.ArticleId,
                    Quantity = item.Quantity
                };

                await _stockServiceClient.RegisterPendingStockAsync(pendingDto);
            }
        }
        private async Task<int> RegisterDispatch(int purchaseId, DateTime date, string origin, string code)
        {
            var dispatch = new Dispatch
            {
                PurchaseId = purchaseId,
                Date = date,
                Origin = origin,
                Code = code
            };

            await _dispatchRepository.AddAsync(dispatch);
            await _purchaseRepository.SaveChangesAsync(); // Asegurarse de guardar los cambios después de agregar el despacho

            return dispatch.Id;
        }

        public async Task<int> CreateAsync(PurchaseCreateDTO dto, int userId)
        {
            // --- External validations (keep existing rules) ---
            _ = await _catalogServiceClient.GetSupplierNameAsync(dto.SupplierId)
                ?? throw new ArgumentException($"Supplier with ID {dto.SupplierId} does not exist.");

            if (dto.WarehouseId is not null)
            {
                _ = await _catalogServiceClient.GetWarehouseNameAsync(dto.WarehouseId.Value)
                    ?? throw new ArgumentException($"Warehouse with ID {dto.WarehouseId} does not exist.");
            }

            _ = await _userServiceClient.GetUserNameAsync(userId)
                ?? throw new ArgumentException($"User with ID {userId} does not exist.");

            foreach (var a in dto.Articles)
            {
                _ = await _catalogServiceClient.GetArticleNameAsync(a.ArticleId)
                    ?? throw new ArgumentException($"Article with ID {a.ArticleId} does not exist.");
            }

            using var tx = await _purchaseRepository.BeginTransactionAsync();

            try
            {
                // 1) Create Purchase
                var purchase = new Purchase
                {
                    Date = dto.Date,
                    SupplierId = dto.SupplierId,
                    WarehouseId = dto.WarehouseId,
                    UserId = userId,
                    IsImportation = dto.IsImportation,
                    IsDelivered = false,
                    Articles = dto.Articles.Select(a => new Purchase_Article
                    {
                        ArticleId = a.ArticleId,
                        Quantity = a.Quantity,
                        UnitCost = a.UnitCost
                    }).ToList()
                };

                await _purchaseRepository.AddAsync(purchase);
                await _purchaseRepository.SaveChangesAsync();      // ensure purchase.Id

                // 2) Optional: create Dispatch (only for importations)
                int? dispatchId = null;
                if (dto.IsImportation && dto.Dispatch is not null)
                {
                    var dispatch = new Dispatch
                    {
                        PurchaseId = purchase.Id,
                        Date = dto.Dispatch.Date,
                        Origin = dto.Dispatch.Origin,
                        Code = dto.Dispatch.Code
                    };

                    await _dispatchRepository.AddAsync(dispatch);
                    await _purchaseRepository.SaveChangesAsync();  // ensure dispatch.Id
                    dispatchId = dispatch.Id;
                }

                // 3) Optional: create Document via service (same DbContext/transaction)
                if (dto.Document is not null)
                {
                    await _purchaseDocumentService.CreateAsync(
                        purchase.Id,
                        dto.Document,
                        userId
                    );
                }

                // 4) Stock impact (now or pending)
                if (dto.RegisterStockNow)
                {
                    if (!dto.WarehouseId.HasValue)
                        throw new ArgumentException("WarehouseId is required when RegisterStockNow = true.");

                    // Register incoming stock
                    await _stockServiceClient.RegisterPurchaseMovementsAsync(
                        userId,
                        dto.WarehouseId.Value,
                        dto.Articles.Select(a => (a.ArticleId, a.Quantity)).ToList(),
                        dto.reference,
                        dispatchId
                    );

                    purchase.IsDelivered = true;
                    await _purchaseRepository.SaveChangesAsync();
                }
                else
                {
                    // Register pending stock per article
                    foreach (var item in dto.Articles)
                    {
                        await _stockServiceClient.RegisterPendingStockAsync(new PendingStockEntryCreateDTO
                        {
                            PurchaseId = purchase.Id,
                            ArticleId = item.ArticleId,
                            Quantity = item.Quantity
                        });
                    }
                }

                await tx.CommitAsync();
                return purchase.Id;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


        public async Task<IEnumerable<PurchaseDTO>> GetPendingStockAsync()
        {
            var purchases = await _purchaseRepository.GetPendingStockAsync();
            return await MapToDTOListAsync(purchases);
        }
        public async Task RegisterStockFromPendingAsync(int purchaseId, int warehouseId, string reference, int userId, int? dispatchId)
        {
            var purchase = await _purchaseRepository.GetByIdAsync(purchaseId);
            if (purchase == null)
                throw new ArgumentException($"Purchase with ID {purchaseId} not found.");

            if (purchase.IsDelivered)
                throw new InvalidOperationException("This purchase has already been marked as delivered.");

            await _stockServiceClient.RegisterPendingStockConsolidatedAsync(new RegisterPendingStockInputDTO
            {
                PurchaseId = purchaseId,
                WarehouseId = warehouseId,
                Reference = reference,
                DispatchId = dispatchId
            }, userId);

            purchase.IsDelivered = true; // Marcar como entregada
            purchase.WarehouseId = warehouseId; // Actualizar el almacén
            purchase.UpdatedAt = DateTime.UtcNow; // Actualizar la fecha de actualización

            await _purchaseRepository.SaveChangesAsync();
        }
        private async Task<IEnumerable<PurchaseDTO>> MapToDTOListAsync(IEnumerable<Purchase> purchases)
        {
            var purchaseDTOs = new List<PurchaseDTO>();
            foreach (var purchase in purchases)
            {
                var purchaseDTO = await MapToDTOAsync(purchase);
                purchaseDTOs.Add(purchaseDTO);
            }
            return purchaseDTOs;
        }
        private async Task<PurchaseDTO> MapToDTOAsync(Purchase purchase)
        {
            var supplierName = await _catalogServiceClient.GetSupplierNameAsync(purchase.SupplierId) ?? "N/A";

            var warehouseName = null as string;
            if (purchase.WarehouseId != null)
            {
                warehouseName = await _catalogServiceClient.GetWarehouseNameAsync(purchase.WarehouseId.Value) ?? "N/A";
            }

            var userName = await _userServiceClient.GetUserNameAsync(purchase.UserId) ?? "N/A";

            var articleDTOs = new List<PurchaseArticleDTO>();
            foreach (var article in purchase.Articles)
            {
                var articleName = await _catalogServiceClient.GetArticleNameAsync(article.ArticleId) ?? "N/A";
                articleDTOs.Add(new PurchaseArticleDTO
                {
                    ArticleId = article.ArticleId,
                    ArticleName = articleName,
                    Quantity = article.Quantity,
                    UnitCost = article.UnitCost
                });
            }

            var hasInvoice = await _purchaseDocumentRepository.ExistsInvoiceAsync(purchase.Id, onlyActive: true);


            return new PurchaseDTO
            {
                Id = purchase.Id,
                Date = purchase.Date,
                SupplierId = purchase.SupplierId,
                SupplierName = supplierName,
                WarehouseId = purchase.WarehouseId,
                WarehouseName = warehouseName,
                UserId = purchase.UserId,
                UserName = userName,
                HasInvoice = hasInvoice,
                //HasDispatch = purchase.Dispatch != null,
                //StockUpdated = purchase.StockUpdated,
                Articles = articleDTOs,
                IsImportation = purchase.IsImportation,
                IsDelivered = purchase.IsDelivered,
                Dispatch = purchase.Dispatch != null ? new DispatchDTO
                {
                    Id = purchase.Dispatch.Id,
                    Code = purchase.Dispatch.Code,
                    Origin = purchase.Dispatch.Origin,
                    Date = purchase.Dispatch.Date
                } : null
            };
        }
        public async Task<IEnumerable<ArticlePurchaseHistoryDTO>> GetPurchaseHistoryByArticleIdAsync(int articleId)
        {
            var records = await _purchaseRepository.GetByArticleIdAsync(articleId);

            return records.Select(pa => new ArticlePurchaseHistoryDTO
            {
                PurchaseDate = pa.Purchase.Date,
                Quantity = pa.Quantity,
                UnitCost = pa.UnitCost
            });
        }

    }
}
