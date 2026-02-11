namespace SalesService.Infrastructure.Models.Sale
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public bool IsFinalConsumer { get; set; } = false;
        public string CustomerIdType { get; set; } = "CUIT";
        public int? CustomerId { get; set; }
        public string CustomerTaxId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int? CustomerPostalCodeId { get; set; }
        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Observations { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool HasInvoice { get; set; } = false;
        public bool IsFullyDelivered { get; set; } = false;

        // Stock warning flags
        // Indicates if this sale currently has any active stock warning.
        public bool HasStockWarning { get; set; } = false;

        // Last time the stock warning state was updated (created or resolved).
        public DateTime? StockWarningUpdatedAt { get; set; }

        public List<Sale_Article> Articles { get; set; } = new();

        // List of stock warnings associated with this sale.
        public List<SaleStockWarning> StockWarnings { get; set; } = new();

        public List<DeliveryNote> DeliveryNotes { get; set; } = new();
    }
}
