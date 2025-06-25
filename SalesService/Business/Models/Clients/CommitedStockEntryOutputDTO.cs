namespace SalesService.Business.Models.Clients
{
    public class CommitedStockEntryOutputDTO
    {
        public int SaleId { get; set; }
        public int WarehouseId { get; set; }
        public string Reference { get; set; } = string.Empty;
        public List<CommitedStockEntryDispatchOutputDTO> Dispatches { get; set; } = new List<CommitedStockEntryDispatchOutputDTO>();

    }

    public class CommitedStockEntryDispatchOutputDTO
    {
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; } = 0;
        public int DispatchId { get; set; }
    }
}
