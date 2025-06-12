namespace SalesService.Business.Models.Clients
{
    public class DispatchStockDetailDTO
    {
        public int? DispatchId { get; set; } // null si fue compra local
        public decimal Quantity { get; set; }
    }
}
