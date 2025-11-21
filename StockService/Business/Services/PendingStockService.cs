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
                Quantity = dto.Quantity,
                UnitCost = dto.UnitCost
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

            // 1) Deltas de cantidad por artículo
            var perArticleDelta = dto.Items
                .GroupBy(i => i.ArticleId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.NewQuantity - x.OldQuantity)  // total delta for that article in this purchase
                );

            // 2) Nuevo costo por artículo (si se envió alguno no nulo)
            var perArticleCost = dto.Items
                .Where(i => i.NewUnitCost.HasValue)
                .GroupBy(i => i.ArticleId)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().NewUnitCost!.Value // asumimos mismo costo para todas las filas del artículo
                );

            // Conjunto total de artículos involucrados (por cantidad y/o costo)
            var articleIds = new HashSet<int>(dto.Items.Select(i => i.ArticleId));

            // 3) Aplicar ajustes en PendingStockEntries (FIFO dentro de esta compra/artículo)
            foreach (var articleId in articleIds)
            {
                var delta = perArticleDelta.TryGetValue(articleId, out var d) ? d : 0m;
                var hasCostChange = perArticleCost.TryGetValue(articleId, out var newCost);

                // Traemos las pending actuales de esta compra/artículo
                var entries = await _pendingRepository.GetUnprocessedByPurchaseArticleAsync(dto.PurchaseId, articleId);

                // --- Ajuste de cantidades ---
                if (delta != 0)
                {
                    if (delta > 0)
                    {
                        // Aumentar pending
                        if (entries.Count > 0)
                        {
                            // Opción simple: sumar al último registro pendiente
                            var last = entries[^1];
                            last.Quantity += delta;
                        }
                        else
                        {
                            // No había pending para esta compra/artículo → crear nueva
                            var entry = new PendingStockEntry
                            {
                                PurchaseId = dto.PurchaseId,
                                ArticleId = articleId,
                                Quantity = delta,
                                IsProcessed = false,
                                CreatedAt = DateTime.UtcNow,
                                // Si hay nuevo costo para este artículo, lo usamos. Si no, 0 (caso raro).
                                UnitCost = hasCostChange ? newCost : 0m
                            };

                            await _pendingRepository.AddAsync(entry);
                        }
                    }
                    else
                    {
                        // Reducir pending: consumir FIFO
                        var remainingToRemove = -delta;
                        foreach (var e in entries)
                        {
                            if (remainingToRemove <= 0) break;

                            var take = Math.Min(e.Quantity, remainingToRemove);
                            e.Quantity -= take;
                            remainingToRemove -= take;
                        }

                        // Eliminar filas que quedaron en 0
                        foreach (var e in entries.Where(x => x.Quantity == 0).ToList())
                        {
                            _pendingRepository.Remove(e);
                        }
                    }
                }

                // --- Ajuste de costo ---
                // Si vino NewUnitCost para este artículo:
                if (hasCostChange)
                {
                    // Volvemos a pedir las pending vivas (por si se creó/borro alguna)
                    var entriesToUpdate = await _pendingRepository.GetUnprocessedByPurchaseArticleAsync(dto.PurchaseId, articleId);

                    if (entriesToUpdate.Count > 0)
                    {
                        // Si todas ya tienen ese costo, no hace falta tocar nada
                        var allAlreadySame = entriesToUpdate.All(e => e.UnitCost == newCost);

                        if (!allAlreadySame)
                        {
                            foreach (var e in entriesToUpdate)
                            {
                                e.UnitCost = newCost;
                            }
                        }
                    }
                }
                // Si NO hay NewUnitCost para el artículo → no tocamos el UnitCost existente.
            }

            // Persistimos cambios en pending antes de calcular conflictos
            await _pendingRepository.SaveChangesAsync();

            // 4) Conflictos por artículo DESPUÉS de aplicar ajustes de pending (solo si cambió cantidad)
            foreach (var kvp in perArticleDelta)
            {
                var articleId = kvp.Key;
                var delta = kvp.Value;
                if (delta == 0) continue;   // cambio solo de costo, no afecta disponibilidad

                // on-hand debería venir de tu snapshot real de stock
                var onHand = await GetOnHandAsync(articleId); // TODO: wire actual on-hand provider

                var pendingNow = await _pendingRepository.SumUnprocessedByArticleAsync(articleId);
                var committedRemaining = await _committedRepository.SumRemainingByArticleAsync(articleId);

                // Aproximamos AvailableBefore (ya que persistimos antes)
                var availableBefore = onHand + (pendingNow - delta) - committedRemaining;
                var availableAfter = onHand + pendingNow - committedRemaining;

                if (availableAfter < 0)
                {
                    var fifo = await _committedRepository.ListRemainingByArticleAsync(articleId);

                    // Faltante global para este artículo:
                    var totalShortage = -availableAfter;

                    var implicated = new List<StockConflictSaleRefDTO>();

                    foreach (var (salesOrderId, remaining) in fifo)
                    {
                        implicated.Add(new StockConflictSaleRefDTO
                        {
                            SaleId = salesOrderId,
                            RemainingCommitted = remaining
                        });
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
