namespace SalesService.Business.Models.Sale
{
    public class SaleDTO
    {
        public int Id { get; set; }
        public bool IsFinalConsumer { get; set; }
        public string? CustomerName { get; set; }
        public string SellerName { get; set; }
        public DateTime Date {  get; set; }
        public decimal Total { get; set; }
        public decimal ExchangeRate { get; set; }
        public bool IsFullyDelivered { get; set; }
        public bool HasInvoice { get; set; }
        public bool HasStockWarning { get; set; }
    }
}
