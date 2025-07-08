namespace StockService.Business.Models.StockTransfer
{
    public class StockTransferCreateDTO
    {
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Code { get; set; } = string.Empty;

        public int OriginWarehouseId { get; set; }
        public int DestinationWarehouseId { get; set; }
        public int? TransportId { get; set; }

        public int NumberOfPackages { get; set; }
        public decimal DeclaredValue { get; set; }

        public string? Observations { get; set; }

        public List<StockTransferArticleCreateDTO> Articles { get; set; } = new List<StockTransferArticleCreateDTO>();

    }
}
