
using PurchaseService.Business.Models;

namespace PurchaseService.Business.Interfaces
{
    public interface IUserServiceClient
    {
        Task<List<UserDTO>> GetAllUsersAsync();
        Task<string?> GetUserNameAsync(int userId);
    }
}
