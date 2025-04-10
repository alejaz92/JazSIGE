namespace PurchaseService.Business.Models
{
    public class PurchaseArticleCreateDTO
    {
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }
}
