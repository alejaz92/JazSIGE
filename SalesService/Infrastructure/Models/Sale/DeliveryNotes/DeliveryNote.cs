namespace SalesService.Infrastructure.Models.Sale
{ 
    public class DeliveryNote
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int WarehouseId { get; set; }
        public int TransportId { get; set; }
        public DateTime Date { get; set; }
        public string? Code { get; set; }
        public string? Observations { get; set; }

        public Sale Sale { get; set; }
        public List<DeliveryNote_Article> Articles { get; set; } = new();
    }
}
