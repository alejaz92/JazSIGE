namespace PurchaseService.Infrastructure.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int SupplierId { get; set; }  
        public int UserId { get; set; }
        //public bool StockUpdated { get; set; } = false;

        public bool IsDelivered { get; set; } = false;
        public int? WarehouseId { get; set; }

        public bool IsImportation { get; set; } = false;
        public Dispatch? Dispatch { get; set; }

        public ICollection<Purchase_Article> Articles { get; set; } = new List<Purchase_Article>();    
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
