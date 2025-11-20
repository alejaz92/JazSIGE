using StockService.Infrastructure.Models;

namespace StockService.Business.Models
{
    public class StockMovementCreateDTO
    {
        public StockMovementType MovementType { get; set; }
        public int ArticleId { get; set; }
        public int? FromWarehouseId { get; set; }
        public int? ToWarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public string? Reference { get; set; }
        public int? DispatchId { get; set; }

        public int? StockTransferId { get; set; }

        public decimal? UnitCost { get; set; }
    }
}
