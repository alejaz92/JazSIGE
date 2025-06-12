namespace SalesService.Business.Models.Clients
{
    public class StockMovementCreateDTO
    {
        public int ArticleId { get; set; }
        public int? FromWarehouseId { get; set; }
        public int? ToWarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public string? Reference { get; set; }
        public int? DispatchId { get; set; }
        public int MovementType { get; set; }
    }
}
