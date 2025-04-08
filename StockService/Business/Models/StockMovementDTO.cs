using StockService.Infrastructure.Models;

namespace StockService.Business.Models
{
    public class StockMovementDTO
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public StockMovementType MovementType { get; set; }

        public int ArticleId { get; set; }
        public string? ArticleName { get; set; }
        public int? FromWarehouseId { get; set; }
        public string? FromWarehouseName { get; set; }
        public int? ToWarehouseId { get; set; }
        public string? ToWarehouseName { get; set; }
        public decimal Quantity { get; set; }
        public string? Reference { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }

    }
}
