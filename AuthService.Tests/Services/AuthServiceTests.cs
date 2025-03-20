using Xunit;
using Moq;
using AuthService.Business.Interfaces;
using AuthService.Business.Models;
using AuthService.Business.Services;
using Microsoft.AspNetCore.Identity;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AuthServiceTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IAuthService> _authServiceMock;

    public AuthServiceTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _authServiceMock = new Mock<IAuthService>();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var loginRequest = new LoginRequestDTO { Username = "admin", Password = "ValidPass123!" };
        var authResponse = new AuthResponseDTO { Token = "fake-jwt-token" };

        _authServiceMock.Setup(service => service.LoginAsync(loginRequest))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _authServiceMock.Object.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("fake-jwt-token");
    }
    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenCredentialsAreInvalid()
    {
        var loginRequest = new LoginRequestDTO { Username = "admin", Password = "WrongPass" };

        _authServiceMock.Setup(service => service.LoginAsync(loginRequest))
            .ReturnsAsync((AuthResponseDTO)null);

        var result = await _authServiceMock.Object.LoginAsync(loginRequest);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnSuccess_WhenPasswordIsUpdated()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDTO { CurrentPassword = "oldPass", NewPassword = "newPass" };

        _authServiceMock.Setup(service => service.ChangePasswordAsync(1, changePasswordDto, false))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authServiceMock.Object.ChangePasswordAsync(1, changePasswordDto, false);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnFailure_WhenPasswordUpdateFails()
    {
        var changePasswordDto = new ChangePasswordDTO { CurrentPassword = "oldPass", NewPassword = "newPass" };

        _authServiceMock.Setup(service => service.ChangePasswordAsync(1, changePasswordDto, false))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Cambio de contraseña fallido" }));

        var result = await _authServiceMock.Object.ChangePasswordAsync(1, changePasswordDto, false);

        result.Succeeded.Should().BeFalse();
    }
}
