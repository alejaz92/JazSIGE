using System.Reflection.Metadata;

namespace SalesService.Business.Models.SalesOrder
{
    public class SalesOrderDTO
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }



        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerTaxId { get; set; } = string.Empty ;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerIVAType {  get; set; } = string.Empty;
        public string CustomerSellCondition {  get; set; } = string.Empty;
        public string CustomerLocation {  get; set; } = string.Empty;

        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;

        public int? TransportId { get; set; }
        public string TransportName { get; set; } = "";

        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = "";

        public int PriceListId { get; set; }
        public decimal ExchangeRate { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        
        public string? Notes { get; set; }
        public bool IsDelivered { get; set; }

        public List<SalesOrder_ArticleDTO> Articles { get; set; } = new();
    }
}
