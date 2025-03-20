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

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default-key"));
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
        public async Task<IdentityResult> ChangePasswordAsync(int userId, ChangePasswordDTO dto, bool isAdmin)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user is null || string.IsNullOrEmpty(dto.NewPassword))
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            if (isAdmin)
            {
                var token = await _userRepository.GeneratePasswordResetTokenAsync(user);
                return await _userRepository.ResetPasswordAsync(user, token, dto.NewPassword);
            }

            //Para usuario normal, es obligatorio enviar la contraseña actual
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                return IdentityResult.Failed(new IdentityError { Description = "Current password is required" });

            return await _userRepository.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

        }

      
    }
}
