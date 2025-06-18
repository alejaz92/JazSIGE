namespace PurchaseService.Business.Models
{
    public class RegisterStockInputDTO
    {
        public int WarehouseId { get; set; }
        public string Reference { get; set; } = string.Empty;
        public int? DispatchId { get; set; }
    }
}
