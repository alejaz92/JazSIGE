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
    //[Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDTO createUserDto)
        {
            if (createUserDto.Role != "Admin" && createUserDto.Role != "Seller")
                return BadRequest("El rol indicado no es válido. Roles permitidos: Admin, Seller.");

            var result = await _userService.CreateUserAsync(createUserDto);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok($"Usuario '{createUserDto.Username}' creado correctamente con rol '{createUserDto.Role}'.");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDTO dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int loggedUserId))
                return Unauthorized("Usuario no autenticado.");

            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && loggedUserId != id)
                return Forbid("No tienes permisos para actualizar este usuario.");

            var result = await _userService.UpdateUserAsync(id, dto, isAdmin);

            if (!result.Succeeded)
                return NotFound("Usuario no encontrado.");

            return Ok("Usuario actualizado correctamente.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, UpdateUserRoleDTO dto)
        {
            if (dto.Role != "Admin" && dto.Role != "Seller")
                return BadRequest("El rol indicado no es válido. Roles permitidos: Admin, Seller.");

            var result = await _userService.UpdateUserRoleAsync(id, dto.Role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok($"Rol actualizado correctamente a '{dto.Role}'.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, UpdateUserStatusDTO dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int loggedUserId))
                return Unauthorized("Usuario no autenticado.");

            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin)
                return Forbid("No tienes permisos para realizar esta acción.");

            var result = await _userService.UpdateUserStatusAsync(id, dto.IsActive);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok($"Estado de usuario actualizado a: {dto.IsActive}");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int loggedUserId))
                return Unauthorized("Usuario no autenticado.");

            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin)
                return Forbid("No tienes permisos para eliminar usuarios.");

            var result = await _userService.DeleteUserAsync(id);

            if (!result.Succeeded)
                return NotFound("Usuario no encontrado.");

            return Ok("Usuario eliminado correctamente.");
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int loggedUserId))
                return Unauthorized("Usuario no autenticado.");

            var user = await _userService.GetUserByIdAsync(id);
            if (user is null)
                return NotFound("Usuario no encontrado.");
            return Ok(user);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int loggedUserId))
                return Unauthorized("Usuario no autenticado.");

            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
    }
}
