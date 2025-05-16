namespace SalesService.Business.Models
{
    public class ArticlePriceListCreateDTO
    {
        public int ArticleId { get; set; }
        public int PriceListId { get; set; }
        public decimal Price { get; set; }
    }
}
