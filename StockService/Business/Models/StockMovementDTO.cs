using StockService.Infrastructure.Models;

namespace StockService.Business.Models
{
    public class StockMovementDTO
    {
        public StockMovementType MovementType { get; set; }
        public int ArticleId { get; set; }
        public int? FromWarehouseId { get; set; }
        public int? ToWarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public string? Reference { get; set; }
    }
}
