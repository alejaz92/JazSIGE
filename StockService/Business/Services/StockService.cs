using AuthService.Infrastructure.Models;
using StockService.Business.Interfaces;
using StockService.Business.Models;
using StockService.Business.Models.CommitedStock;
using StockService.Infrastructure.Interfaces;
using StockService.Infrastructure.Models;

namespace StockService.Business.Services
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly IStockByDispatchRepository _stockByDispatchRepository;
        private readonly ICatalogServiceClient _catalogServiceClient;
        private readonly IUserServiceClient _userServiceClient;
        //private readonly ICommitedStockService _commitedStockService;
        //private readonly IPendingStockService _pendingStockService;

        public StockService(
            IStockRepository stockRepository,
            IStockMovementRepository stockMovementRepository,
            IStockByDispatchRepository stockByDispatchRepository,
            ICatalogServiceClient catalogServiceClient,
            IUserServiceClient userServiceClient)
        {
            _stockRepository = stockRepository;
            _stockMovementRepository = stockMovementRepository;
            _stockByDispatchRepository = stockByDispatchRepository;
            _catalogServiceClient = catalogServiceClient;
            _userServiceClient = userServiceClient;
            //_commitedStockService = commitedStockService;
            //_pendingStockService = pendingStockService;
        }

        public async Task<List<DispatchStockDetailDTO>> RegisterMovementAsync(StockMovementCreateDTO dto, int userId)
        {
            if (!await _catalogServiceClient.ArticleExistsAsync(dto.ArticleId))
            {
                throw new ArgumentException($"Article with ID {dto.ArticleId} does not exist.");
            }
            // validate involved warehouses
            if (dto.FromWarehouseId != null && !await _catalogServiceClient.WarehouseExistsAsync(dto.FromWarehouseId.Value))
            {
                throw new ArgumentException($"From Warehouse with ID {dto.FromWarehouseId} does not exist.");
            }
            if (dto.ToWarehouseId != null && !await _catalogServiceClient.WarehouseExistsAsync(dto.ToWarehouseId.Value))
            {
                throw new ArgumentException($"To Warehouse with ID {dto.ToWarehouseId} does not exist.");
            }

            // validar que al restar stock de un deposito, ese deposito no tenga un stock menor al que se quiere restar
            if (dto.MovementType == StockMovementType.Sale || dto.MovementType == StockMovementType.Transfer)
            {
                var currentStock = await _stockRepository.GetByArticleAndwarehouseAsync(dto.ArticleId, dto.FromWarehouseId.Value);
                if (currentStock == null || currentStock.Quantity < dto.Quantity)
                {
                    throw new InvalidOperationException($"Not enough stock in warehouse {dto.FromWarehouseId} for article {dto.ArticleId}.");
                }
            }

            // if it is purchase, calculate average cost
            // get current stock, last average cost and calculate new average cost

            decimal? newAvgCost = null;
            if (dto.MovementType == StockMovementType.Purchase)
            {
                var currentStock = await _stockRepository.GetCurrentStockByArticleAsync(dto.ArticleId);
                decimal currentAvgCost = 0m;
                decimal currentQuantity = 0m;
                if (currentStock != null)
                {
                    // get last stock movement to get average cost
                    var lastMovement = await _stockMovementRepository.GetLastByArticleAndMovementTypeAsync(dto.ArticleId, dto.MovementType);
                    if (lastMovement != null && lastMovement.AvgUnitCost.HasValue)
                    {
                        currentAvgCost = lastMovement.AvgUnitCost.Value;
                        currentQuantity = currentStock;
                    }
                }
                newAvgCost = ((currentAvgCost * currentQuantity) + (dto.UnitCost.GetValueOrDefault() * dto.Quantity)) / (currentQuantity + dto.Quantity);
                
            }

            var stockMovement = new StockMovement
            {
                ArticleId = dto.ArticleId,
                MovementType = dto.MovementType,
                Quantity = dto.Quantity,
                LastUnitCost = dto.UnitCost,
                AvgUnitCost = newAvgCost,
                Date = DateTime.UtcNow,
                FromWarehouseId = dto.FromWarehouseId,
                ToWarehouseId = dto.ToWarehouseId,
                Reference = dto.Reference,
                UserId = userId
            };

            // Register the stock movement
            await _stockMovementRepository.AddAsync(stockMovement);


            // Update stock based on the movement type
            switch (dto.MovementType)
            {
                case StockMovementType.Purchase:
                //case StockMovementType.Adjustment:
                    if (!dto.ToWarehouseId.HasValue)
                        throw new ArgumentException("ToWarehouseId is required for this movement.");
                    await UpdateStockAsync(dto.ArticleId, dto.ToWarehouseId.Value, dto.Quantity);

                    await UpdateStockByDispatchAsync(dto.ArticleId, dto.DispatchId, dto.Quantity);
                    break;

                case StockMovementType.Sale:
                    if (!dto.FromWarehouseId.HasValue)
                        throw new ArgumentException("FromWarehouseId is required for this movement.");

                    var breakdown = await DiscountStockByDispatchAsync(dto.ArticleId, dto.Quantity);
                    await UpdateStockAsync(dto.ArticleId, dto.FromWarehouseId.Value, -dto.Quantity);

                    return breakdown;

                case StockMovementType.Adjustment:
                    if (dto.FromWarehouseId.HasValue)
                    {
                        await UpdateStockAsync(dto.ArticleId, dto.FromWarehouseId.Value, -dto.Quantity);

                        // Ajuste negativo → usar lógica FIFO
                        var dispatch = await DiscountStockByDispatchAsync(dto.ArticleId, dto.Quantity);
                        // Podés loguear que esto fue ajuste, no venta
                    }
                    else if (dto.ToWarehouseId.HasValue)
                    {
                        await UpdateStockAsync(dto.ArticleId, dto.ToWarehouseId.Value, dto.Quantity);

                        // Ajuste positivo → agregar al último despacho disponible
                        var latestEntry = await _stockByDispatchRepository.GetLatestByArticleAsync(dto.ArticleId);
                        if (latestEntry != null)
                        {
                            latestEntry.Quantity += dto.Quantity;
                            await _stockByDispatchRepository.UpdateAsync(latestEntry);
                        }
                        else
                        {
                            // No hay entradas previas → creamos con DispatchId null
                            var newEntry = new StockByDispatch
                            {
                                ArticleId = dto.ArticleId,
                                DispatchId = null,
                                Quantity = dto.Quantity
                            };
                            await _stockByDispatchRepository.AddAsync(newEntry);
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Either FromWarehouseId or ToWarehouseId must be provided for adjustments.");
                    }
                    break;

                case StockMovementType.Transfer:
                    if (!dto.FromWarehouseId.HasValue || !dto.ToWarehouseId.HasValue)
                        throw new ArgumentException("Both FromWarehouseId and ToWarehouseId are required for transfers.");

                    // Actualizar stock en ambos depósitos
                    await UpdateStockAsync(dto.ArticleId, dto.FromWarehouseId.Value, -dto.Quantity);
                    await UpdateStockAsync(dto.ArticleId, dto.ToWarehouseId.Value, dto.Quantity);

                    break;

            }

            return new List<DispatchStockDetailDTO>();


        }
        private async Task UpdateStockAsync(int articleId, int warehouseId, decimal quantityChange)
        {
            var stock = await _stockRepository.GetByArticleAndwarehouseAsync(articleId, warehouseId);

            if (stock == null)
            {
                if (quantityChange < 0)
                {
                    throw new InvalidOperationException("Cannot create a stock record with negative quantity.");
                }

                stock = new Stock
                {
                    ArticleId = articleId,
                    WarehouseId = warehouseId,
                    Quantity = quantityChange,
                    UpdatedAt = DateTime.UtcNow
                };
                await _stockRepository.AddAsync(stock);
            }
            else
            {
                stock.Quantity += quantityChange;
                stock.UpdatedAt = DateTime.UtcNow;
                await _stockRepository.UpdateAsync(stock);
            }
        }
        public async Task<decimal> GetStockSummaryAsync(int articleId)
        {
            var stockList = await _stockRepository.GetAllByArticleAsync(articleId);
            return stockList.Sum(s => s.Quantity);
        }
        public async Task<IEnumerable<StockDTO>> GetStockByArticleAsync(int articleId)
        {
            var stockList = await _stockRepository.GetAllByArticleAsync(articleId);
            var tasks = stockList.Select(async s =>
            {
                var warehouseName = await _catalogServiceClient.GetWarehouseNameAsync(s.WarehouseId);
                return new StockDTO
                {
                    WarehouseId = s.WarehouseId,
                    Quantity = s.Quantity,
                    WarehouseName = warehouseName
                };
            });
            return await Task.WhenAll(tasks);
        }
        public async Task<IEnumerable<StockByWarehouseDTO>> GetStockByWarehouseAsync(int warehouseId)
        {
            var stockList = await _stockRepository.GetAllByWarehouseAsync(warehouseId);
            var tasks = stockList.Select(async s =>
            {
                var article = await _catalogServiceClient.GetArticleAsync(s.ArticleId);
                return new StockByWarehouseDTO
                {
                    ArticleID = s.ArticleId,
                    ArticleName = article.Description,
                    ArticleSKU = article.SKU,
                    ArticleLineId = article.LineId,
                    ArticleLine = article.Line,
                    ArticleLineGroupId = article.LineGroupId,
                    ArticleLineGroup = article.LineGroup,
                    ArticleBrandId = article.BrandId,
                    ArticleBrand = article.Brand,
                    Quantity = s.Quantity
                };
            });
            return await Task.WhenAll(tasks);
        }
        public async Task<decimal> GetStockAsync(int articleId, int warehouseId)
        {
            var stock = await _stockRepository.GetByArticleAndwarehouseAsync(articleId, warehouseId);
            return stock?.Quantity ?? 0m;
        }
        public async Task<PaginatedResultDTO<StockMovementDTO>> GetMovementsByArticleAsync(int articleId, int page, int pageSize)
        {
            var (movements, totalCount) = await _stockMovementRepository.GetPagedWithTotalAsync(articleId, page, pageSize);
            var articleName = await _catalogServiceClient.GetArticleNameAsync(articleId);

            var tasks = movements.Select(async m =>
            {
                var fromName = m.FromWarehouseId.HasValue
                    ? await _catalogServiceClient.GetWarehouseNameAsync(m.FromWarehouseId.Value)
                    : null;

                var toName = m.ToWarehouseId.HasValue
                    ? await _catalogServiceClient.GetWarehouseNameAsync(m.ToWarehouseId.Value)
                    : null;

                var userName = await _userServiceClient.GetUserNameAsync(m.UserId);

                return new StockMovementDTO
                {
                    Id = m.Id,
                    Date = m.Date,
                    MovementType = m.MovementType,
                    StockMovementTypeName = m.MovementType.ToString(),
                    ArticleId = m.ArticleId,
                    ArticleName = articleName,
                    FromWarehouseId = m.FromWarehouseId,
                    FromWarehouseName = fromName,
                    ToWarehouseId = m.ToWarehouseId,
                    ToWarehouseName = toName,
                    Quantity = m.Quantity,
                    Reference = m.Reference,
                    UserId = m.UserId,
                    UserName = userName
                };
            });

            var items = await Task.WhenAll(tasks);

            return new PaginatedResultDTO<StockMovementDTO>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        private async Task UpdateStockByDispatchAsync(int articleId, int? dispatchId, decimal quantity)
        {
            var entry = await _stockByDispatchRepository.GetByArticleAndDispatchAsync(articleId, dispatchId);

            if (entry != null)
            {
                entry.Quantity += quantity;
                await _stockByDispatchRepository.UpdateAsync(entry);
            }
            else
            {
                var newEntry = new StockByDispatch
                {
                    ArticleId = articleId,
                    DispatchId = dispatchId,
                    Quantity = quantity
                };
                await _stockByDispatchRepository.AddAsync(newEntry);
            }
        }
        private async Task<List<DispatchStockDetailDTO>> DiscountStockByDispatchAsync(int articleId, decimal quantityToSubtract)
        {
            var dispatchEntries = await _stockByDispatchRepository.GetAvailableByArticleOrderedAsync(articleId);
            var result = new List<DispatchStockDetailDTO>();
            decimal remaining = quantityToSubtract;

            foreach (var entry in dispatchEntries)
            {
                if (remaining <= 0) break;

                var deducted = Math.Min(entry.Quantity, remaining);
                entry.Quantity -= deducted;
                remaining -= deducted;

                await _stockByDispatchRepository.UpdateAsync(entry);

                result.Add(new DispatchStockDetailDTO
                {
                    DispatchId = entry.DispatchId,
                    Quantity = deducted
                });
            }

            if (remaining > 0)
            {
                throw new InvalidOperationException($"Not enough stock available for article {articleId} across dispatches.");
            }

            return result;
        }        

        

    }

}
