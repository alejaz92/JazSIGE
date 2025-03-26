namespace CatalogService.Infrastructure.Models
{
    public class Article : BaseEntity
    {
        public string Description { get; set; }
        public string SKU { get; set; }
        public int BrandId { get; set; }
        public Brand Brand { get; set; }
        public int LineId { get; set; }
        public Line Line { get; set; }
        public int UnitId { get; set; }
        public Unit Unit { get; set; }
        public bool IsTaxed { get; set; }
        public decimal IVAPercentage { get; set; }
        public int GrossIncomeTypeId { get; set; }
        public GrossIncomeType GrossIncomeType { get; set; }
        public int Warranty {  get; set; }
        public bool IsVisible { get; set; }


    }
}
