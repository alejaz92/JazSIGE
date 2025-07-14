namespace FiscalDocumentationService.Infrastructure.Models
{
    public class FiscalDocumentArticle
    {
        public int Id { get; set; }

        public int FiscalDocumentId { get; set; }
        public FiscalDocument FiscalDocument { get; set; } = null!;

        public int ArticleId { get; set; }
        public string ArticleName { get; set; } = null!;
        public string ArticleSKU { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal IVAPercent { get; set; }
    }
}
