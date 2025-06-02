namespace SalesService.Business.Models.SalesOrder
{
    public class SalesOrderListDTO
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string CustomerName { get; set; } = "";
        public string SellerName { get; set; } = "";
        public decimal Total { get; set; }
        public decimal ExchangeRate { get; set; }
        public bool IsDelivered { get; set; }
    }
}
