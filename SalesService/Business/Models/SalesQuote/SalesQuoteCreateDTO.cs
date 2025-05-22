namespace SalesService.Business.Models.SalesQuote
{
    public class SalesQuoteCreateDTO
    {
        public DateTime Date { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int CustomerId { get; set; }
        public int SellerId { get; set; }
        public int TransportId { get; set; }
        public int PriceListId { get; set; }
        public decimal ExchangeRate { get; set; }
        public string? Observations { get; set; }

        public List<SalesQuoteArticleCreateDTO> Articles { get; set; } = new();
    }
}
