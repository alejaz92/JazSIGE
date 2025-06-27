namespace SalesService.Business.Models.DeliveryNote
{
    public class DeliveryNoteCreateDTO
    {
        public string Code { get; set; } = string.Empty;
        public int SaleId { get; set; }
        public int WarehouseId { get; set; }
        public int TransportId { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string? Observations { get; set; }
        public decimal DeclaredValue { get; set; } = 0;
        public int NumberOfPackages { get; set; } = 0;



        public List<DeliveryNoteArticleCreateDTO> Articles { get; set; } = new();
    }

    public class DeliveryNoteArticleCreateDTO
    {
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
    }
}
