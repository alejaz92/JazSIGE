namespace AuthService.Business.Models
{
    public class UpdateUserDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public decimal? SalesCommission { get; set; }
    }
}
