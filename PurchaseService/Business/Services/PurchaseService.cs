﻿using PurchaseService.Business.Exceptions;
using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Models;
using PurchaseService.Infrastructure.Interfaces;
using PurchaseService.Infrastructure.Models;

namespace PurchaseService.Business.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IDispatchRepository _dispatchRepository;
        private readonly ICatalogServiceClient _catalogServiceClient;
        private readonly IUserServiceClient _userServiceClient;
        private readonly IStockServiceClient _stockServiceClient;

        public PurchaseService(
            IPurchaseRepository purchaseRepository,
            IDispatchRepository dispatchRepository,
            ICatalogServiceClient catalogServiceClient, 
            IUserServiceClient userServiceClient,
            IStockServiceClient stockServiceClient
            )
        {
            _purchaseRepository = purchaseRepository;
            _dispatchRepository = dispatchRepository;
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
            if (purchase == null)  return null;
            
            return await MapToDTOAsync(purchase);
        }

        private async Task UpdateStockConsolidated(int userId, int warehouseId, string  reference, int? dispatchId, List<PurchaseArticleCreateDTO> articles)
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
            //validate supplier
            var supplierName = await _catalogServiceClient.GetSupplierNameAsync(dto.SupplierId);
            if (supplierName == null)
                throw new ArgumentException($"Supplier with ID {dto.SupplierId} does not exist.");

            //validate warehouse
            if (dto.WarehouseId != null)
            {
                var warehouseName = await _catalogServiceClient.GetWarehouseNameAsync(dto.WarehouseId.Value);
                if (warehouseName == null)
                    throw new ArgumentException($"Warehouse with ID {dto.WarehouseId} does not exist.");
            }
            
            //validate user
            var userName = await _userServiceClient.GetUserNameAsync(userId);
            if (userName == null)
                throw new ArgumentException($"User with ID {userId} does not exist.");

            //validate articles
            foreach (var article in dto.Articles)
            {
                var articleName = await _catalogServiceClient.GetArticleNameAsync(article.ArticleId);
                if (articleName == null)
                    throw new ArgumentException($"Article with ID {article.ArticleId} does not exist.");
            }


            using var transaction = await _purchaseRepository.BeginTransactionAsync();

            try
            {
                var purchase = new Purchase
                {
                    Date = dto.Date,
                    SupplierId = dto.SupplierId,
                    WarehouseId = dto.WarehouseId,
                    UserId = userId,
                    IsImportation = dto.IsImportation,
                    IsDelivered = false, // se actualizará después si corresponde
                    Articles = dto.Articles.Select(a => new Purchase_Article
                    {
                        ArticleId = a.ArticleId,
                        Quantity = a.Quantity,
                        UnitCost = a.UnitCost
                    }).ToList()
                };

                await _purchaseRepository.AddAsync(purchase);
                await _purchaseRepository.SaveChangesAsync(); // Necesario para obtener el ID


                var dispatchId = (int?)null;
                // Hay despacho?
                if (dto.Dispatch != null && dto.IsImportation)
                {
                    dispatchId = await RegisterDispatch(purchase.Id, dto.Dispatch.Date, dto.Dispatch.Origin, dto.Dispatch.Code);
                }

                // registramos el stock ahora?
                if (dto.RegisterStockNow)
                {
                    await UpdateStockConsolidated(userId, dto.WarehouseId.Value, dto.reference, dispatchId, dto.Articles);

                    purchase.IsDelivered = true;
                    await _purchaseRepository.SaveChangesAsync();
                }
                else
                {

                    await UpdateStockPending(purchase.Id, dto.Articles);

                }


                await transaction.CommitAsync();

                return purchase.Id;

            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        //public async Task RetryStockUpdateAsync(int purchaseId, int userId)
        //{
        //    var purchase = await _purchaseRepository.GetByIdAsync(purchaseId);
        //    if (purchase == null)
        //        throw new ArgumentException($"Purchase with ID {purchaseId} does not exist.");
        //    if (purchase.StockUpdated)
        //        throw new InvalidOperationException($"Stock for purchase with ID {purchaseId} has already been updated.");

        //    // Revalidar existencia de artículos (opcional, podés saltearlo si estás seguro)
        //    foreach (var pa in purchase.Articles)
        //    {
        //        var exists = await _catalogServiceClient.GetArticleNameAsync(pa.ArticleId);
        //        if (exists == null)
        //            throw new ArgumentException($"Article with ID {pa.ArticleId} no longer exists.");
        //    }


        //    await _stockServiceClient.RegisterPurchaseMovementsAsync(
        //        userId,
        //        //purchase.WarehouseId,
        //        purchase.Articles.Select(a => (a.ArticleId, a.Quantity)).ToList(),
        //        "ReTried stock movement",
        //        null
        //    );
        //    purchase.StockUpdated = true;
        //    purchase.UpdatedAt = DateTime.UtcNow;
        //    await _purchaseRepository.SaveChangesAsync();
            
           
        //}

        //public async Task<int> RetryAllPendingStockAsync(int userId)
        //{
        //    var purchases = await _purchaseRepository.GetPendingStockAsync();
        //    int successCount = 0;

        //    foreach (var purchase in purchases)
        //    {
        //        try
        //        {
        //            await _stockServiceClient.RegisterPurchaseMovementsAsync(
        //                userId,
        //                purchase.WarehouseId,
        //                purchase.Articles.Select(a => (a.ArticleId, a.Quantity)).ToList(),
        //                "ReTried Stock Movement",
        //                null
        //            );

        //            purchase.StockUpdated = true;
        //            purchase.UpdatedAt = DateTime.UtcNow;
        //            successCount++;
        //        }
        //        catch
        //        {
        //            // Loguear o ignorar individualmente
        //        }
        //    }

        //    await _purchaseRepository.SaveChangesAsync(); // guardar todos los cambios juntos

        //    return successCount;
        //}


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
    }
}
