namespace SalesService.Business.Models.SalesOrder
{
    public class DispatchStockDetailDTO
    {
        public int? DispatchId { get; set; } // null si fue compra local
        public decimal Quantity { get; set; }
    }
}
