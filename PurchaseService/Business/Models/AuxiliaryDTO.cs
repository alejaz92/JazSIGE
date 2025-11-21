namespace PurchaseService.Business.Models
{
    public class SupplierDTO { public string CompanyName { get; set; } = ""; }
    public class WarehouseDTO { public string Description { get; set; } = ""; }
    public class ArticleDTO { public string Description { get; set; } = ""; }
    public class UserDTO
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
    }

    public class StockMovementCreateDTO
    {
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public int MovementType { get; set; }
        public int? FromWarehouseId { get; set; }
        public int? ToWarehouseId { get; set; }
        public string? Reference { get; set; }
        public int? DispatchId { get; set; }
    }
}
