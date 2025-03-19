using AuthService.Infrastructure.Interfaces;
using AuthService.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public UserRepository(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<User> GetUserByUsernameAsync(string username) => await _userManager.FindByNameAsync(username);

        public async Task<bool> CheckPasswordAsync(User user, string password) => await _userManager.CheckPasswordAsync(user, password);

        public async Task<string> GeneratePasswordResetTokenAsync(User user) => await _userManager.GeneratePasswordResetTokenAsync(user);

        // Nuevo método: Resetear contraseña utilizando un token (Admin)
        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
        {
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<IList<string>> GetRolesAsync(User user) => await _userManager.GetRolesAsync(user);

        public async Task<IdentityResult> CreateUserAsync(User user, string password, string role)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return result;

            // Verifica que el rol existe, si no, lo crea.
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole<int>(role));

            // Asigna rol al usuario.
            return await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<IdentityResult> AddRoleAsync(User user, string role) => await _userManager.AddToRoleAsync(user, role);

        public async Task<User?> GetUserByIdAsync(int id) => await _userManager.FindByIdAsync(id.ToString());

        public async Task<IdentityResult> UpdateUserAsync(User user) => await _userManager.UpdateAsync(user);

        public async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<IList<string>> GetUserRolesAsync(User user) => await _userManager.GetRolesAsync(user);

        public async Task<IdentityResult> RemoveUserFromRolesAsync(User user, IEnumerable<string> roles) => await _userManager.RemoveFromRolesAsync(user, roles);

        public async Task<IdentityResult> AddUserToRoleAsync(User user, string role)
        {
            if(!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole<int>(role));

            return await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<IdentityResult> DeleteUserAsync(User user) => await _userManager.DeleteAsync(user);

        public async Task<List<User>> GetAllUsersAsync() => await _userManager.Users.ToListAsync();
    }
}
