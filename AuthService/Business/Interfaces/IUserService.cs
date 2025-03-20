using AuthService.Business.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Business.Interfaces
{
    public interface IUserService
    {
        Task<IdentityResult> CreateUserAsync(CreateUserDTO createUserDTO);
        Task<IdentityResult> DeleteUserAsync(int userId);
        Task<UserDTO?> GetUserByIdAsync(int userId);
        Task<List<UserDTO>> GetAllUsersAsync();
        Task<IdentityResult> UpdateUserAsync(int userId, UpdateUserDTO dto, bool isAdmin);
        Task<IdentityResult> UpdateUserRoleAsync(int userId, string newRole);
        Task<IdentityResult> UpdateUserStatusAsync(int userId, bool isActive);
    }
}
