namespace SalesService.Business.Models.SalesOrder
{
    public class SalesOrderCreateDTO
    {
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public int SellerId { get; set; }
        public int PriceListId { get; set; }
        public int TransportId { get; set; }
        public int WarehouseId { get; set; }
        public string? Notes { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public List<SalesOrder_ArticleCreateDTO> Articles { get; set; } = new();
    }

}
