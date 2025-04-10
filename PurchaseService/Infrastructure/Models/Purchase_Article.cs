namespace PurchaseService.Infrastructure.Models
{
    public class Purchase_Article
    {
        public int PurchaseId { get; set; }
        public int ArticleId { get; set; }

        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }

        public Purchase Purchase { get; set; }
    }
}
