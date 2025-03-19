using AuthService.Business.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Business.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> ChangePasswordAsync(int userId, ChangePasswordDTO dto, bool isAdmin);
        Task<IdentityResult> CreateUserAsync(CreateUserDTO createUserDTO);
        Task<IdentityResult> DeleteUserAsync(int userId);
        Task<UserDTO?> GetUserByIdAsync(int userId);
        Task<List<UserDTO>> GetUsersAsync();
        Task<AuthResponseDTO?> LoginAsync(LoginRequestDTO loginRequest);
        Task<IdentityResult> UpdateUserAsync(int userId, UpdateUserDTO dto, bool isAdmin);
        Task<IdentityResult> UpdateUserRoleAsync(int userId, string newRole);
        Task<IdentityResult> UpdateUserStatusAsync(int userId, bool isActive);
    }
}
