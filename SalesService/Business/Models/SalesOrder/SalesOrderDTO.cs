namespace SalesService.Business.Models.SalesOrder
{
    public class SalesOrderDTO
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";

        public int SellerId { get; set; }
        public string SellerName { get; set; } = "";

        public int? TransportId { get; set; }
        public string TransportName { get; set; } = "";

        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = "";

        public string? Notes { get; set; }
        public bool IsDelivered { get; set; }

        public decimal ExchangeRate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }

        public List<SalesOrder_ArticleDTO> Articles { get; set; } = new();
    }
}
