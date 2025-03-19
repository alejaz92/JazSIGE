using AuthService.Business.Interfaces;
using AuthService.Business.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;
        public UserController(IAuthService authService)
        {
            _authService = authService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDTO createUserDto)
        {
            if (createUserDto.Role != "Admin" && createUserDto.Role != "Seller")
                return BadRequest("El rol indicado no es válido. Roles permitidos: Admin, Seller.");

            var result = await _authService.CreateUserAsync(createUserDto);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok($"Usuario '{createUserDto.Username}' creado correctamente con rol '{createUserDto.Role}'.");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDTO dto)
        {
            var loggedUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && loggedUserId != id)
                return Forbid("No puedes editar otro usuario.");

            var result = await _authService.UpdateUserAsync(id, dto, isAdmin);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Usuario actualizado correctamente.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, UpdateUserRoleDTO dto)
        {
            if (dto.Role != "Admin" && dto.Role != "Seller")
                return BadRequest("El rol indicado no es válido. Roles permitidos: Admin, Seller.");

            var result = await _authService.UpdateUserRoleAsync(id, dto.Role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok($"Rol actualizado correctamente a '{dto.Role}'.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, UpdateUserStatusDTO dto)
        {
            var result = await _authService.UpdateUserStatusAsync(id, dto.IsActive);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            
            var status = dto.IsActive ? "activado" : "desactivado";
            return Ok($"Usuario {status} correctamente.");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _authService.DeleteUserAsync(id);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return Ok("Usuario eliminado correctamente.");
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            var loggedUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //var isAdmin = User.IsInRole("Admin");

            //if (!isAdmin && loggedUserId != id)
            //    return Forbid("No puedes ver otro usuario.");

            var user = await _authService.GetUserByIdAsync(id);
            if (user is null)
                return NotFound("Usuario no encontrado.");
            return Ok(user);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var loggedUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //var isAdmin = User.IsInRole("Admin");
            //if (!isAdmin)
            //    return Forbid("No puedes ver todos los usuarios.");
            var users = await _authService.GetAllUsersAsync();
            return Ok(users);
        }
    }
}
