namespace StockService.Business.Models.StockTransfer
{
    public class StockTransferDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public int OriginWarehouseId { get; set; }
        public int DestinationWarehouseId { get; set; }
        public int? TransportId { get; set; }

        public int NumberOfPackages { get; set; }
        public decimal DeclaredValue { get; set; }

        public string? Observations { get; set; }
        public int UserId { get; set; }

        public List<StockTransferArticleDTO> Articles { get; set; } = new();
    }
}
