namespace CatalogService.Business.Models.Line
{
    public class LineDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int LineGroupId { get; set; }
        public string LineGroupDescription { get; set; }
    }
}
