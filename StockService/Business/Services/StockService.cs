using AuthService.Infrastructure.Models;
using StockService.Business.Interfaces;
using StockService.Business.Models;
using StockService.Infrastructure.Interfaces;
using StockService.Infrastructure.Models;

namespace StockService.Business.Services
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly ICatalogValidatorService _catalogValidatorService;
        private readonly IUserServiceClient _userServiceClient;

        public StockService(
            IStockRepository stockRepository,
            IStockMovementRepository stockMovementRepository,
            ICatalogValidatorService catalogValidatorService,
            IUserServiceClient userServiceClient)
        {
            _stockRepository = stockRepository;
            _stockMovementRepository = stockMovementRepository;
            _catalogValidatorService = catalogValidatorService;
            _userServiceClient = userServiceClient;
        }

        public async Task RegisterMovementAsync(StockMovementDTO dto, int userId)
        {
            if (!await _catalogValidatorService.ArticleExistsAsync(dto.ArticleId))
            {
                throw new ArgumentException($"Article with ID {dto.ArticleId} does not exist.");
            }
            // validate involved warehouses
            if (dto.FromWarehouseId != null && !await _catalogValidatorService.WarehouseExistsAsync(dto.FromWarehouseId.Value))
            {
                throw new ArgumentException($"From Warehouse with ID {dto.FromWarehouseId} does not exist.");
            }
            if (dto.ToWarehouseId != null && !await _catalogValidatorService.WarehouseExistsAsync(dto.ToWarehouseId.Value))
            {
                throw new ArgumentException($"To Warehouse with ID {dto.ToWarehouseId} does not exist.");
            }

            var stockMovement = new StockMovement
            {
                ArticleId = dto.ArticleId,
                MovementType = dto.MovementType,
                Quantity = dto.Quantity,
                Date = DateTime.UtcNow,
                FromWarehouseId = dto.FromWarehouseId,
                ToWarehouseId = dto.ToWarehouseId,
                Reference = dto.Reference,
                UserId = userId
            };
            await _stockMovementRepository.AddAsync(stockMovement);

            // Update stock based on the movement type
            switch (dto.MovementType)
            {
                case StockMovementType.Purchase:
                case StockMovementType.Adjustment:
                case StockMovementType.TransferIn:
                    if (!dto.ToWarehouseId.HasValue)
                        throw new ArgumentException("ToWarehouseId must be provided for this movement type.");
                    await UpdateStockAsync(dto.ArticleId, dto.ToWarehouseId.Value, dto.Quantity);
                    break;
                case StockMovementType.TransferOut:
                case StockMovementType.Sale:
                    if (!dto.FromWarehouseId.HasValue)
                        throw new ArgumentException("FromWarehouseId must be provided for this movement type.");
                    await UpdateStockAsync(dto.ArticleId, dto.FromWarehouseId.Value, -dto.Quantity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dto.MovementType), "Unknown stock movement type.");

            }

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

        public async Task<decimal> GetStockAsync(int articleId, int warehouseId)
        {
            var stock = await _stockRepository.GetByArticleAndwarehouseAsync(articleId, warehouseId);
            return stock?.Quantity ?? 0m;
        }

        public async Task<IEnumerable<StockMovementDetailDTO>> GetMovementsByArticleAsync(int articleId, int page, int pageSize)
        {
            var movements = await _stockMovementRepository.GetPagedByArticleAsync(articleId, page, pageSize);
            var articleName = await _catalogValidatorService.GetArticleNameAsync(articleId);

            var tasks = movements.Select(async m =>
            {
                var fromName = m.FromWarehouseId.HasValue
                    ? await _catalogValidatorService.GetWarehouseNameAsync(m.FromWarehouseId.Value)
                    : null;

                var toName = m.ToWarehouseId.HasValue
                    ? await _catalogValidatorService.GetWarehouseNameAsync(m.ToWarehouseId.Value)
                    : null;

                var userName = await _userServiceClient.GetUserNameAsync(m.UserId);

                return new StockMovementDetailDTO
                {
                    Id = m.Id,
                    Date = m.Date,
                    MovementType = m.MovementType,
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

            return await Task.WhenAll(tasks);
        }
    }

}
