namespace AuthService.Infrastructure.Models
{
    public class Stock
    {
        public int Id { get; set; }
        public int ArticleId {  get; set; }
        public int WarehouseId { get; set; }
        public decimal Quantity { get; set; }

        public DateTime UpdatedAt { get; set; }

    }
}
