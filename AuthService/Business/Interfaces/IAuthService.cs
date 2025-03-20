using AuthService.Business.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Business.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> ChangePasswordAsync(int userId, ChangePasswordDTO dto, bool isAdmin);
        Task<AuthResponseDTO?> LoginAsync(LoginRequestDTO loginRequest);

    }
}
