namespace SalesService.Business.Models.DeliveryNote
{
    public class DeliveryNoteCreateDTO
    {
        public int SaleId { get; set; }
        public int WarehouseId { get; set; }
        public int TransportId { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string? Observations { get; set; }
        public List<DeliveryNoteArticleCreateDTO> Articles { get; set; } = new();
    }

    public class DeliveryNoteArticleCreateDTO
    {
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
    }
}
