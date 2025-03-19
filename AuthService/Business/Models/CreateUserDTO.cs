namespace AuthService.Business.Models
{
    public class CreateUserDTO
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public bool IsActive { get; set; }
        public decimal SalesCommission { get; set; }
        public string Role { get; set; } = null!; // "Admin" o "Seller"
    }
}
