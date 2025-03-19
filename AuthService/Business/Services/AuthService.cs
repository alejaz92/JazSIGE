using AuthService.Business.Interfaces;
using AuthService.Business.Models;
using AuthService.Infrastructure.Interfaces;
using AuthService.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDTO?> LoginAsync(LoginRequestDTO loginRequest)
        {
            var user = await _userRepository.GetUserByUsernameAsync(loginRequest.Username);
            if (user is null || !await _userRepository.CheckPasswordAsync(user, loginRequest.Password))
                return null;

            var roles = await _userRepository.GetRolesAsync(user);

            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!)
        };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new AuthResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };
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

        public async Task<IdentityResult> ChangePasswordAsync(int userId, ChangePasswordDTO dto, bool isAdmin)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user is null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            if(isAdmin)
            {
                var token = await _userRepository.GeneratePasswordResetTokenAsync(user);
                return await _userRepository.ResetPasswordAsync(user, token, dto.NewPassword);
            }

            //Para usuario normal, es obligatorio enviar la contraseña actual
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                return IdentityResult.Failed(new IdentityError { Description = "Current password is required" });

            return await _userRepository.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

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

        public async Task<List<UserDTO>> GetUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return users.Select(user => new UserDTO
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                SalesCommission = user.SalesCommission
            }).ToList();
        }

        public async Task<UserDTO?> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user is null)
                return null;
            return new UserDTO
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                SalesCommission = user.SalesCommission
            };
        }
    }
}
