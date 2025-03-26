namespace CatalogService.Business.Models.Article
{
    public class ArticleCreateDTO
    {
        public string Description { get; set; }
        public string SKU { get; set; }
        public int BrandId { get; set; }
        public int LineId { get; set; }
        public int UnitId { get; set; }
        public bool IsTaxed { get; set; }
        public decimal IVAPercentage { get; set; }
        public int GrossIncomeTypeId { get; set; }
        public int Warranty { get; set; }
        public bool IsVisible { get; set; }
    }
}
