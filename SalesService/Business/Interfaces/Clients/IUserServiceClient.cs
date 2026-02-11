
using SalesService.Business.Models.Clients;

namespace SalesService.Business.Interfaces.Clients
{
    public interface IUserServiceClient
    {
        Task<List<UserDTO>> GetAllUsersAsync();
        Task<UserDTO?> GetUserByIdAsync(int userId);
    }
}
