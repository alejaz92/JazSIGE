using AuthService.Business.Interfaces;
using AuthService.Business.Models;
using AuthService.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginRequestDTO loginRequest)
        {
            var response = await _authService.LoginAsync(loginRequest);
            if (response == null)
                return Unauthorized("Credenciales inválidas");

            return Ok(response);
        }

        //[HttpPost("crear-admin-inicial")]
        //public async Task<IActionResult> CrearAdminInicial([FromServices] UserManager<User> userManager,
        //                                           [FromServices] RoleManager<IdentityRole<int>> roleManager)
        //{
        //    // Verificar si ya existe usuario admin
        //    var userExists = await userManager.FindByNameAsync("admin");
        //    if (userExists != null)
        //        return BadRequest("El usuario admin ya existe.");

        //    // Crear nuevo usuario admin con tus propiedades adicionales
        //    var adminUser = new User
        //    {
        //        UserName = "admin",
        //        FirstName = "Administrador",
        //        LastName = "Principal",
        //        IsActive = true,
        //        SalesCommission = 0m,
        //        Email = "admin@admin.com"
        //    };

        //    var result = await userManager.CreateAsync(adminUser, "Admin123"); // Cambia la clave luego

        //    if (!result.Succeeded)
        //        return BadRequest(result.Errors);

        //    // Verificar rol Admin y asignarlo
        //    if (!await roleManager.RoleExistsAsync("Admin"))
        //        await roleManager.CreateAsync(new IdentityRole<int>("Admin"));

        //    await userManager.AddToRoleAsync(adminUser, "Admin");

        //    return Ok("Usuario administrador inicial creado correctamente.");
        //}       


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("change-password/{id:int}")]
        public async Task<IActionResult> ChangePassword(int id, ChangePasswordDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var loggedUserId = int.Parse(userIdClaim.Value);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && loggedUserId != id)
                return Forbid("No puedes cambiar la contraseña de otro usuario.");

            var result = await _authService.ChangePasswordAsync(id, dto, isAdmin);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Contraseña actualizada correctamente.");
        }
    }
}
