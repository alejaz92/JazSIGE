using StockService.Business.Interfaces;

namespace StockService.Business.Services
{
    public class StockAvailabilityService : IStockAvailabilityService
    {
        private readonly IStockService _stockService;
        private readonly ICommitedStockService _commitedStockService;
        private readonly IPendingStockService _pendingStockService;

        public StockAvailabilityService(
            IStockService stockService,
            ICommitedStockService commitedStockService,
            IPendingStockService pendingStockService)
        {
            _stockService = stockService;
            _commitedStockService = commitedStockService;
            _pendingStockService = pendingStockService;
        }

        public async Task<decimal> GetAvailableStockByArticleAsync(int articleId)
        {
            var currentStock = await _stockService.GetStockSummaryAsync(articleId);
            var pendingStock = await _pendingStockService.GetPendingStockByArticleAsync(articleId);
            var commitedStock = await _commitedStockService.GetTotalCommitedStockByArticleIdAsync(articleId);

            return currentStock - pendingStock - commitedStock.Total;
        }

        public async Task<decimal> GetAvailableStockByArticleAndWarehouseAsync(int articleId, int warehouseId)
        {
            // Physical stock total (excluding pending)
            var totalStock = await _stockService.GetStockSummaryAsync(articleId);

            // Physical stock in the selected warehouse
            var warehouseStock = await _stockService.GetStockAsync(articleId, warehouseId);

            // Pending incoming stock (global)
            var pendingStock = await _pendingStockService.GetPendingStockByArticleAsync(articleId);

            // Total committed stock (global, without warehouse assignment)
            var committedStock = await _commitedStockService.GetTotalCommitedStockByArticleIdAsync(articleId);

            // Stock from other warehouses
            var otherWarehousesStock = Math.Max(0m, totalStock - warehouseStock);

            // Capacity to cover commitments without touching the selected warehouse
            var coverFromOthers = otherWarehousesStock + pendingStock;

            // Remaining committed stock that must be covered by the selected warehouse
            var committedRemaining = Math.Max(0m, committedStock.Total - coverFromOthers);

            // Available stock in the selected warehouse
            var availableStock = Math.Max(0m, warehouseStock - committedRemaining);

            return availableStock;
        }
    }
}
