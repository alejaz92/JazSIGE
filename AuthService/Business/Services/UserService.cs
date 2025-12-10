using AuthService.Business.Interfaces;
using AuthService.Business.Models;
using AuthService.Infrastructure.Interfaces;
using AuthService.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<IdentityResult> CreateUserAsync(CreateUserDTO createUserDTO)
        {
            var user = new User
            {
                UserName = createUserDTO.Username,
                Email = createUserDTO.Email,
                FirstName = createUserDTO.FirstName,
                LastName = createUserDTO.LastName,
                IsActive = true,
                SalesCommission = createUserDTO.SalesCommission
            };
            return await _userRepository.CreateUserAsync(user, createUserDTO.Password, createUserDTO.Role);
        }

        public async Task<IdentityResult> UpdateUserAsync(int userId, UpdateUserDTO dto, bool isAdmin)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user is null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;
            if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email;

            if (isAdmin)
            {
                if (dto.SalesCommission.HasValue) user.SalesCommission = dto.SalesCommission.Value;
            }

            return await _userRepository.UpdateUserAsync(user);

        }

        public async Task<IdentityResult> UpdateUserRoleAsync(int userId, string newRole)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user is null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            var currentRoles = await _userRepository.GetUserRolesAsync(user);

            var removeResult = await _userRepository.RemoveUserFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return removeResult;

            var addResult = await _userRepository.AddUserToRoleAsync(user, newRole);

            return addResult;
        }

        public async Task<IdentityResult> UpdateUserStatusAsync(int userId, bool isActive)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user is null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            user.IsActive = isActive;
            return await _userRepository.UpdateUserAsync(user);
        }

        public async Task<IdentityResult> DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user is null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            return await _userRepository.DeleteUserAsync(user);
        }

        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var userDTOs = new List<UserDTO>();
            foreach (var user in users)
            {
                var roles = await _userRepository.GetUserRolesAsync(user);
                var isAdmin = roles.Contains("Admin");
                userDTOs.Add(new UserDTO
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    SalesCommission = user.SalesCommission,
                    IsAdmin = isAdmin
                });
            }
            return userDTOs;
        }

        public async Task<UserDTO?> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user is null)
                return null;
            // get roles
            var roles = await _userRepository.GetUserRolesAsync(user);
            var isAdmin = roles.Contains("Admin");

            return new UserDTO
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                SalesCommission = user.SalesCommission,
                IsAdmin = isAdmin
            };
        }

    }
}
