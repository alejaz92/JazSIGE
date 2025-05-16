namespace SalesService.Infrastructure.Models
{
    public class ArticlePriceList 
    {
        public int Id { get; set; } 
        public int ArticleId { get; set; }
        public int PriceListId { get; set; }
        public decimal Price { get; set; }
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt {  get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set;} = DateTime.UtcNow;
    }
}
