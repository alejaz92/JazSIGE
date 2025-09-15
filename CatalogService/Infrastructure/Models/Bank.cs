namespace CatalogService.Infrastructure.Models
{
    public class Bank : BaseEntity
    {
        public string Name { get; set; } = null!;
        public int Code { get; set; }
    }
}
