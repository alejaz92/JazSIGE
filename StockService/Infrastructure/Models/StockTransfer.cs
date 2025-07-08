namespace StockService.Infrastructure.Models
{
    public class StockTransfer
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;

        public int OriginWarehouseId { get; set; }
        public int DestinationWarehouseId { get; set; }
        public int? TransportId { get; set; }

        public int NumberOfPackages { get; set; }
        public decimal DeclaredValue { get; set; }

        public string? Observations { get; set; }
        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<StockTransfer_Article> Articles { get; set; } = new List<StockTransfer_Article>();

    }
}
