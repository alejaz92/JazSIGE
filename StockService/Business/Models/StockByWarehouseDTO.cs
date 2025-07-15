namespace StockService.Business.Models
{
    public class StockByWarehouseDTO
    {
        public int ArticleID { get; set; }  
        public string ArticleName { get; set; }
        public string ArticleSKU { get; set; }
        public int ArticleLineId { get; set; }
        public string ArticleLine { get; set; }
        public int ArticleLineGroupId { get; set; }
        public string ArticleLineGroup { get; set; }
        public int ArticleBrandId { get; set; }
        public string ArticleBrand { get; set; }
        public decimal Quantity { get; set; }
    }
}
