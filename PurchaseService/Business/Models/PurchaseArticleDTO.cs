namespace PurchaseService.Business.Models
{
    public class PurchaseArticleDTO
    {
        public int ArticleId { get; set; }
        public string ArticleName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }
}
