namespace PurchaseService.Business.Models
{
    public class RegisterPendingStockInputDTO
    {
        public int PurchaseId { get; set; }
        public int WarehouseId { get; set; }
        public string Reference { get; set; } = string.Empty;
        public int? DispatchId { get; set; }
    }
}
