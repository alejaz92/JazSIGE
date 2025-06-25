namespace StockService.Business.Models.CommitedStock
{
    public class RegisterCommitedStockOutputDTO
    {
        public int SaleId { get; set; }
        public int WarehouseId { get; set; }
        public string Reference { get; set; }
        public List<RegisterCommitedStockDispatchOutputDTO> Dispatches { get; set; } = new List<RegisterCommitedStockDispatchOutputDTO>();
    }

    public class RegisterCommitedStockDispatchOutputDTO
    {
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
        public int DispatchId { get; set; }
    }
}
