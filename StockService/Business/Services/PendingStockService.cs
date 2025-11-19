using AuthService.Infrastructure.Models;
using StockService.Business.Interfaces;
using StockService.Business.Models;
using StockService.Business.Models.Clients;
using StockService.Business.Models.CommitedStock;
using StockService.Business.Models.PendingStock;
using StockService.Infrastructure.Interfaces;
using StockService.Infrastructure.Models;

namespace StockService.Business.Services
{
    public class PendingStockService : IPendingStockService
    {
        private readonly IPendingStockEntryRepository _pendingRepository;
        private readonly ICommitedStockEntryRepository _committedRepository;
        private readonly IStockMovementRepository _movementRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IStockByDispatchRepository _dispatchRepository;
        private readonly ISalesServiceClient _salesClient;

        public PendingStockService(
            IPendingStockEntryRepository pendingRepository,
            ICommitedStockEntryRepository committedRepository,
            IStockMovementRepository movementRepository,
            IStockRepository stockRepository,
            IStockByDispatchRepository dispatchRepository,
            ISalesServiceClient salesClient)
        {
            _pendingRepository = pendingRepository;
            _committedRepository = committedRepository;
            _movementRepository = movementRepository;
            _stockRepository = stockRepository;
            _dispatchRepository = dispatchRepository;
            _salesClient = salesClient;
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

        public async Task<decimal> GetPendingStockByArticleAsync(int articleId)
        {
            var pendingEntriesSum = await _pendingRepository.GetTotalPendingStockByArticleIdAsync(articleId);
            return pendingEntriesSum;

        }

        // Applies the pending adjustments for a purchase, never blocks,
        // and returns a conflict report if availableAfter < 0 for any article.
        public async Task<StockApplyAdjustmentResultDTO> ApplyPurchasePendingAdjustmentsAsync(PurchasePendingAdjustmentDTO dto)
        {
            var result = new StockApplyAdjustmentResultDTO { PurchaseId = dto.PurchaseId };
            var perArticleDelta = dto.Items
                .GroupBy(i => i.ArticleId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.NewQuantity - x.OldQuantity)  // total delta for that article in this purchase
                );

            // 1) Apply adjustments in PendingStockEntries (FIFO within this purchase/article)
            foreach (var kvp in perArticleDelta)
            {
                var articleId = kvp.Key;
                var delta = kvp.Value;

                if (delta == 0) continue;

                var entries = await _pendingRepository.GetUnprocessedByPurchaseArticleAsync(dto.PurchaseId, articleId);

                if (delta > 0)
                {
                    // Increase pending: append to last unprocessed entry or create a new one if needed
                    if (entries.Count > 0)
                    {
                        // simplest: add to the last entry
                        var last = entries[^1];
                        last.Quantity += delta;
                    }
                    else
                    {
                        _pendingRepository.AddAsync(new PendingStockEntry
                        {
                            PurchaseId = dto.PurchaseId,
                            ArticleId = articleId,
                            Quantity = delta,
                            IsProcessed = false,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
                else
                {
                    // Reduce pending: consume FIFO entries (only unprocessed of this purchase/article)
                    var remainingToRemove = -delta;
                    foreach (var e in entries)
                    {
                        if (remainingToRemove <= 0) break;

                        var take = Math.Min(e.Quantity, remainingToRemove);
                        e.Quantity -= take;
                        remainingToRemove -= take;
                    }

                    // Clean zero-quantity rows (optional)
                    foreach (var e in entries.Where(x => x.Quantity == 0).ToList())
                    {
                        _pendingRepository.Remove(e);
                    }
                }
            }

            // Persist changes before computing the conflict snapshot, so consumers get the final state
            await _pendingRepository.SaveChangesAsync();

            // 2) Compute conflicts per article AFTER applying deltas
            foreach (var kvp in perArticleDelta)
            {
                var articleId = kvp.Key;
                var delta = kvp.Value;
                if (delta == 0) continue;

                // Compute available BEFORE and AFTER for reporting clarity.
                // NOTE: onHand access should come from your stock snapshot.
                // Replace GetOnHandAsync with your real implementation.
                var onHand = await GetOnHandAsync(articleId); // TODO: wire actual on-hand provider

                var pendingNow = await _pendingRepository.SumUnprocessedByArticleAsync(articleId);
                var committedRemaining = await _committedRepository.SumRemainingByArticleAsync(articleId);

                // We don't have the "before" snapshot anymore after persist.
                // Approximate: AvailableBefore = onHand + (pendingNow - delta) - committedRemaining
                var availableBefore = onHand + (pendingNow - delta) - committedRemaining;
                var availableAfter = onHand + pendingNow - committedRemaining;

                if (availableAfter < 0)
                {
                    var fifo = await _committedRepository.ListRemainingByArticleAsync(articleId);

                    // Faltante global para este artículo:
                    var totalShortage = -availableAfter;

                    //var shortage = totalShortage;
                    var implicated = new List<StockConflictSaleRefDTO>();

                    foreach (var (salesOrderId, remaining) in fifo)
                    {
                        //if (shortage <= 0) break;

                        implicated.Add(new StockConflictSaleRefDTO
                        {
                            SaleId = salesOrderId,
                            RemainingCommitted = remaining
                        });

                        //shortage -= remaining;
                    }

                    result.Conflicts.Add(new StockConflictPerArticleDTO
                    {
                        ArticleId = articleId,
                        AvailableBefore = availableBefore,
                        AvailableAfter = availableAfter,
                        ImplicatedSales = implicated
                    });

                    var warningsToSend = implicated.Select(x => new SaleStockWarningInputDTO
                    {
                        SaleId = x.SaleId,
                        ArticleId = articleId,

                        // 👉 ahora guardamos el faltante global,
                        // el mismo valor para todas las ventas implicadas.
                        ShortageSnapshot = totalShortage
                    }).ToList();

                    await _salesClient.SendStockWarningsAsync(warningsToSend);
                }

            }

            return result;
        }

        // Placeholder until real on-hand source is wired.
        // Replace with repository/service that reads current stock on hand for the article.
        private Task<decimal> GetOnHandAsync(int articleId)
        {
            // TODO: implement with your stock snapshot provider
            return Task.FromResult(0m);
        }
    }
}
