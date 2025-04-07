namespace StockService.Business.Models
{
    public class ArticleDTO
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class WarehouseDTO
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class UserDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
