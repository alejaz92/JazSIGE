namespace StockService.Infrastructure.Models
{
    public class StockMovement
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public StockMovementType MovementType { get; set; }

        public int ArticleId { get; set; }
        public int? FromWarehouseId { get; set; }
        public int? ToWarehouseId { get; set; }
        public decimal Quantity { get; set; }

        public string? Reference { get; set; }
        public int UserId { get; set; }


        //Stock Transfer related properties
        public int? StockTransferId { get; set; }
        public StockTransfer? StockTransfer { get; set; }

        public decimal? LastUnitCost { get; set; }  
        public decimal? AvgUnitCost { get; set; }
        }
}
