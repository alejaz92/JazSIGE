using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using AuthService.Business.Interfaces;
using AuthService.Controllers;
using AuthService.Business.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _authController;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _authController = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WithToken()
    {
        // Arrange
        var loginRequest = new LoginRequestDTO
        {
            Username = "admin",
            Password = "ValidPass123!"
        };

        var authResponse = new AuthResponseDTO
        {
            Token = "fake-jwt-token"
        };

        _authServiceMock.Setup(service => service.LoginAsync(loginRequest))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _authController.LoginAsync(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedToken = Assert.IsType<AuthResponseDTO>(okResult.Value);
        Assert.Equal("fake-jwt-token", returnedToken.Token);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        var loginRequest = new LoginRequestDTO { Username = "admin", Password = "WrongPass" };

        _authServiceMock.Setup(service => service.LoginAsync(loginRequest)).ReturnsAsync((AuthResponseDTO)null);

        var result = await _authController.LoginAsync(loginRequest);

        //Assert.IsType<UnauthorizedResult>(result);
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }


    [Fact]
    public async Task ChangePassword_ShouldReturnOk_WhenPasswordIsUpdated()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDTO
        {
            CurrentPassword = "oldPass",
            NewPassword = "newPass"
        };

        _authServiceMock.Setup(service => service.ChangePasswordAsync(1, changePasswordDto, false))
            .ReturnsAsync(IdentityResult.Success);

        // Configurar el contexto de usuario simulado
        var userClaims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(userClaims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = await _authController.ChangePassword(1, changePasswordDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }


    [Fact]
    public async Task ChangePassword_ShouldReturnBadRequest_WhenPasswordUpdateFails()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDTO
        {
            CurrentPassword = "oldPass",
            NewPassword = "newPass"
        };

        _authServiceMock.Setup(service => service.ChangePasswordAsync(1, changePasswordDto, false))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Cambio de contraseña fallido" }));

        // Configurar el contexto de usuario simulado
        var userClaims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(userClaims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = await _authController.ChangePassword(1, changePasswordDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }
}
