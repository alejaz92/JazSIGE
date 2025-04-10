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
        public string MovementType { get; set; } = null!;
        public int? FromWarehouseId { get; set; }
        public int? ToWarehouseId { get; set; }
        public string? Reference { get; set; }
    }
}
