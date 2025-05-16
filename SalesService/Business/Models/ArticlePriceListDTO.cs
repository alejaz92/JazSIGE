namespace SalesService.Business.Models
{
    public class ArticlePriceListDTO
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public int PriceListId { get; set; }
        public decimal Price { get; set; }
        public DateTime EffectiveFrom { get; set; }
    }
}
