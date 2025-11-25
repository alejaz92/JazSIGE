namespace StockService.Business.Models
{
    public class ArticleCostsDTO
    {
        public DateTime Date { get; set; }
        public decimal Quantity { get; set; }
        public decimal AvgCost { get; set; }
        public decimal LastCost { get; set; }
    }
}
