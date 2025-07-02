namespace PurchaseService.Business.Models
{
    public class ArticlePurchaseHistoryDTO
    {
        public DateTime PurchaseDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }
}
