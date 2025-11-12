namespace PurchaseService.Business.Models
{
    public class PurchaseArticleUpdateDTO
    {
        public int ArticleId { get; set; }  
        public decimal? Quantity { get; set; }
        public decimal? UnitCost { get; set; }
    }
}
