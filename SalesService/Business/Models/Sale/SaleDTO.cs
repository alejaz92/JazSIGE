namespace SalesService.Business.Models.Sale
{
    public class SaleDTO
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string SellerName { get; set; }
        public DateTime Date {  get; set; }
        public decimal Total { get; set; }
    }
}
