using System.Collections.Generic;
using System.Threading.Tasks;
using AuthService.Business.Interfaces;
using AuthService.Business.Models;
using AuthService.Business.Services;
using AuthService.Infrastructure.Interfaces;
using AuthService.Infrastructure.Models;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly IAuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _configurationMock = new Mock<IConfiguration>();

        _authService = new AuthService.Business.Services.AuthService(_userRepositoryMock.Object, _configurationMock.Object);
    }

    //GetAllUsersAsync
    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = 1, UserName = "admin", FirstName = "Admin", LastName = "User", IsActive = true, SalesCommission = 0 },
            new User { Id = 2, UserName = "seller1", FirstName = "Juan", LastName = "Pérez", IsActive = true, SalesCommission = 5 }
        };

        _userRepositoryMock.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(users);
        // Act
        var result = await _authService.GetAllUsersAsync();
        // Assert
        result.Should().HaveCount(2);
        result[0].Username.Should().Be("admin");
        result[1].Username.Should().Be("seller1");
    }

    //GetUserByIdAsync
    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = new User { Id = 2, UserName = "seller1", FirstName = "Juan", LastName = "Pérez", IsActive = true, SalesCommission = 5 };

        _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(2)).ReturnsAsync(user);

        // Act
        var result = await _authService.GetUserByIdAsync(2);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("seller1");
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(99)).ReturnsAsync((User)null!);

        // Act
        var result = await _authService.GetUserByIdAsync(99);

        // Assert
        result.Should().BeNull();
    }

    // change password
    [Fact]
    public async Task ChangePasswordAsync_ShouldChangePassword_WhenCalledByUserWithCorrectPassword()
    {
        // Arrange
        var user = new User { Id = 2, UserName = "seller1" };

        _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(2)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.CheckPasswordAsync(user, "OldPass")).ReturnsAsync(true);
        _userRepositoryMock.Setup(repo => repo.ChangePasswordAsync(user, "OldPass", "NewPass")).ReturnsAsync(IdentityResult.Success);

        var dto = new ChangePasswordDTO { CurrentPassword = "OldPass", NewPassword = "NewPass" };

        // Act
        var result = await _authService.ChangePasswordAsync(2, dto, isAdmin: false);

        // Assert
        result.Succeeded.Should().BeTrue();
        _userRepositoryMock.Verify(repo => repo.ChangePasswordAsync(user, "OldPass", "NewPass"), Times.Once);
    }

    //Create User Async
    [Fact]
    public async Task CreateUserAsync_ShouldReturnSuccess_WhenUserIsCreated()
    {
        // Arrange
        var newUser = new User { Id = 3, UserName = "newUser", FirstName = "New", LastName = "User", IsActive = true };
        var createUserDTO = new CreateUserDTO { Username = "newUser", FirstName = "New", LastName = "User", Password = "Test123!", Role = "Seller" };

        _userRepositoryMock.Setup(repo => repo.CreateUserAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.CreateUserAsync(createUserDTO);

        // Assert
        result.Succeeded.Should().BeTrue();
        _userRepositoryMock.Verify(repo => repo.CreateUserAsync(It.IsAny<User>(), createUserDTO.Password, createUserDTO.Role), Times.Once);
    }


    // Delete User
    [Fact]
    public async Task DeleteUserAsync_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var user = new User { Id = 2, UserName = "seller1" };

        _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(2)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.DeleteUserAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.DeleteUserAsync(2);

        // Assert
        result.Succeeded.Should().BeTrue();
        _userRepositoryMock.Verify(repo => repo.DeleteUserAsync(user), Times.Once);
    }

    //Login Async
    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var loginRequest = new LoginRequestDTO { Username = "admin", Password = "ValidPass123!" };
        var user = new User { Id = 1, UserName = "admin", IsActive = true };

        _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync("admin")).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.CheckPasswordAsync(user, "ValidPass123!")).ReturnsAsync(true);
        _userRepositoryMock.Setup(repo => repo.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

        // Mockear la configuración del token JWT
        _configurationMock.Setup(config => config["Jwt:Key"]).Returns("super-secret-long-key-32-characters");
        _configurationMock.Setup(config => config["Jwt:Issuer"]).Returns("test-issuer");
        _configurationMock.Setup(config => config["Jwt:Audience"]).Returns("test-audience");

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
    }


    //Update User Role
    [Fact]
    public async Task UpdateUserRoleAsync_ShouldUpdateRole_WhenUserExists()
    {
        // Arrange
        var user = new User { Id = 2, UserName = "seller1" };

        _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(2)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.GetUserRolesAsync(user)).ReturnsAsync(new List<string> { "Seller" });
        _userRepositoryMock.Setup(repo => repo.RemoveUserFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userRepositoryMock.Setup(repo => repo.AddUserToRoleAsync(user, "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.UpdateUserRoleAsync(2, "Admin");

        // Assert
        result.Succeeded.Should().BeTrue();
        _userRepositoryMock.Verify(repo => repo.RemoveUserFromRolesAsync(user, It.IsAny<IEnumerable<string>>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.AddUserToRoleAsync(user, "Admin"), Times.Once);
    }

    //Update User Status
    [Fact]
    public async Task UpdateUserStatusAsync_ShouldUpdateStatus_WhenUserExists()
    {
        // Arrange
        var user = new User { Id = 2, UserName = "seller1", IsActive = true };

        _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(2)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.UpdateUserAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.UpdateUserStatusAsync(2, false);

        // Assert
        result.Succeeded.Should().BeTrue();
        user.IsActive.Should().BeFalse();
        _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(user), Times.Once);
    }
}
