namespace SalesService.Infrastructure.Models.SalesOrder
{
    public class SalesOrder
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public int SellerId { get; set; }
        public int PriceListId { get; set; }
        public int? TransportId { get; set; }
        public int WarehouseId { get; set; }
        public string? Notes { get; set; }
        public bool IsDelivered { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public List<SalesOrder_Article> Articles { get; set; } = new();
    }
}
