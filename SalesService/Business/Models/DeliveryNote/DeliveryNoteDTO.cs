namespace SalesService.Business.Models.DeliveryNote
{
    public class DeliveryNoteDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Observations { get; set; }
        
        public string WarehouseName { get; set; } = string.Empty;
        
        public List<DeliveryNoteArticleDTO> Articles { get; set; } = new();

        public decimal DeclaredValue { get; set; } = 0;
        public int NumberOfPackages { get; set; } = 0;

        // Customer information
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerPostalCode { get; set; } = string.Empty;
        public string CustomerCity { get; set; } = string.Empty;
        public string CustomerTaxId { get; set; } = string.Empty;
        public string CustomerSellCondition { get; set; } = string.Empty;
        public string CustomerDeliveryAddress { get; set; } = string.Empty;
        public string CustomerIVAType { get; set; } = string.Empty;

        // transport information
        public int? TransportId { get; set; }
        public string? TransportName { get; set; } = string.Empty;
        public string? TransportAddress { get; set; } = string.Empty;
        public string? TransportCity { get; set; } = string.Empty;
        public string? TransportTaxId { get; set; } = string.Empty;
        public string? TransportPhone { get; set; } = string.Empty;
    }

    public class DeliveryNoteArticleDTO
    {
        public int ArticleId { get; set; }
        public string ArticleName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string? DispatchCode { get; set; }
    }
}
