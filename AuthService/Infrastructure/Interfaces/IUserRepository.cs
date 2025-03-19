using AuthService.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<IList<string>> GetRolesAsync(User user);
        Task<User> GetUserByUsernameAsync(string username);
        Task<IdentityResult> AddRoleAsync(User user, string role);
        Task<IdentityResult> CreateUserAsync(User user, string password, string role);
        Task<User?> GetUserByIdAsync(int id);
        Task<IdentityResult> UpdateUserAsync(User user);
        Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(User user);
        Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);
        Task<IList<string>> GetUserRolesAsync(User user);
        Task<IdentityResult> RemoveUserFromRolesAsync(User user, IEnumerable<string> roles);
        Task<IdentityResult> AddUserToRoleAsync(User user, string role);
        Task<IdentityResult> DeleteUserAsync(User user);
        Task<List<User>> GetAllUsersAsync();
    }
}
