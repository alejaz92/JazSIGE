namespace SalesService.Infrastructure.Models.Sale
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public int SellerId { get; set; }
        public decimal ExchangeRate { get; set; }
        public string Observations { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<Sale_Article> Articles { get; set; } = new();
    }
}
