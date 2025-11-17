namespace SalesService.Business.Models.Sale
{
    public class SaleResolveStockWarningItemDTO
    {
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; } // nueva cantidad
    }

    public class SaleResolveStockWarningDTO
    {
        public List<SaleResolveStockWarningItemDTO> Articles { get; set; } = new();
    }

    public class SaleResolveStockWarningResultDTO
    {
        public bool IsCancelled { get; set; }
        public SaleDetailDTO? Sale { get; set; }
    }
}
