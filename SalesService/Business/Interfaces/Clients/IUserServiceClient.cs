
using SalesService.Business.Models.SalesQuote;

namespace SalesService.Business.Interfaces.Clients
{
    public interface IUserServiceClient
    {
        Task<UserDTO?> GetUserByIdAsync(int userId);
    }
}
