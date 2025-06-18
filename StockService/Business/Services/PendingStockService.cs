using AuthService.Infrastructure.Models;
using StockService.Business.Interfaces;
using StockService.Business.Models;
using StockService.Infrastructure.Interfaces;
using StockService.Infrastructure.Models;

namespace StockService.Business.Services
{
    public class PendingStockService : IPendingStockService
    {
        private readonly IPendingStockEntryRepository _pendingRepository;
        private readonly IStockMovementRepository _movementRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IStockByDispatchRepository _dispatchRepository;

        public PendingStockService(
            IPendingStockEntryRepository pendingRepository,
            IStockMovementRepository movementRepository,
            IStockRepository stockRepository,
            IStockByDispatchRepository dispatchRepository)
        {
            _pendingRepository = pendingRepository;
            _movementRepository = movementRepository;
            _stockRepository = stockRepository;
            _dispatchRepository = dispatchRepository;
        }

        public async Task AddAsync(PendingStockEntryCreateDTO dto)
        {
            var entity = new PendingStockEntry
            {
                PurchaseId = dto.PurchaseId,
                ArticleId = dto.ArticleId,
                Quantity = dto.Quantity
            };

            await _pendingRepository.AddAsync(entity);
        }

        public async Task<List<PendingStockEntryDTO>> GetByPurchaseIdAsync(int purchaseId)
        {
            var entries = await _pendingRepository.GetByPurchaseIdAsync(purchaseId);
            return entries.Select(e => new PendingStockEntryDTO
            {
                Id = e.Id,
                PurchaseId = e.PurchaseId,
                ArticleId = e.ArticleId,
                Quantity = e.Quantity,
                IsProcessed = e.IsProcessed
            }).ToList();
        }

        public async Task RegisterPendingStockAsync(RegisterPendingStockInputDTO dto, int userId)
        {
            var pendingEntries = await _pendingRepository.GetByPurchaseIdAsync(dto.PurchaseId);
            foreach (var entry in pendingEntries)
            {
                // Create stock movement
                var movement = new StockMovement
                {
                    ArticleId = entry.ArticleId,
                    Quantity = entry.Quantity,
                    MovementType = StockMovementType.Purchase,
                    FromWarehouseId = null,
                    ToWarehouseId = dto.WarehouseId,
                    Reference = dto.Reference,
                    UserId = userId
                };
                await _movementRepository.AddAsync(movement);

                // Update stock
                var existingStock = await _stockRepository.GetByArticleAndwarehouseAsync(entry.ArticleId, dto.WarehouseId);
                if (existingStock != null)
                {
                    existingStock.Quantity += entry.Quantity;
                    existingStock.UpdatedAt = DateTime.UtcNow;
                    await _stockRepository.UpdateAsync(existingStock);
                }
                else
                {
                    var stock = new Stock
                    {
                        ArticleId = entry.ArticleId,
                        WarehouseId = dto.WarehouseId,
                        Quantity = entry.Quantity,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _stockRepository.AddAsync(stock);
                }

                // Si hay despacho, actualizar stock por despacho
                if (dto.DispatchId.HasValue)
                {
                    var existing = await _dispatchRepository.GetByArticleAndDispatchAsync(entry.ArticleId, dto.DispatchId.Value);
                    if (existing != null)
                    {
                        existing.Quantity += entry.Quantity;
                        await _dispatchRepository.UpdateAsync(existing);
                    }
                    else
                    {
                        await _dispatchRepository.AddAsync(new StockByDispatch
                        {
                            ArticleId = entry.ArticleId,
                            DispatchId = dto.DispatchId,
                            Quantity = entry.Quantity
                        });
                    }
                }

                await _pendingRepository.MarkAsProcessedAsync(entry.Id);
            }
        }
    }
}
