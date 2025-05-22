public class SalesQuoteListDTO
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public decimal TotalUSD { get; set; }
}
