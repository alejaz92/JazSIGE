namespace SalesService.Business.Models.DeliveryNote
{
    public class DeliveryNoteDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Observations { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string TransportName {  get; set; } = string.Empty;
        public List<DeliveryNoteArticleDTO> Articles { get; set; } = new();
    }

    public class DeliveryNoteArticleDTO
    {
        public int ArticleId { get; set; }
        public string ArticleName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string? DispatchCode { get; set; }
    }
}
