
using SalesService.Business.Models.Clients;

namespace SalesService.Business.Interfaces.Clients
{
    public interface IUserServiceClient
    {
        Task<UserDTO?> GetUserByIdAsync(int userId);
    }
}
