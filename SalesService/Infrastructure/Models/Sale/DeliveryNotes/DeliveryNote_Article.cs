namespace SalesService.Infrastructure.Models.Sale

{
    public class DeliveryNote_Article
    {
        public int Id { get; set; }
        public int DeliveryNoteId { get; set; }
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
        public int? DispatchId  { get; set; }
        public string? DispatchCode { get; set; }

        public DeliveryNote DeliveryNote { get; set; }
    }
}
