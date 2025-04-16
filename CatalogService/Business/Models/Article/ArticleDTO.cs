namespace CatalogService.Business.Models.Article
{
    public class ArticleDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }
        public int BrandId { get; set; }
        public string Brand { get; set; }
        public int LineId { get; set; }
        public string Line { get; set; }
        public int UnitId { get; set; }
        public string Unit { get; set; }
        public bool IsTaxed { get; set; }
        public decimal IVAPercentage { get; set; }
        public int GrossIncomeTypeId { get; set; }
        public string GrossIncomeType { get; set; }
        public int Warranty { get; set; }
        public bool IsVisible { get; set; }
        public bool isActive { get; set; } 
    }
}
