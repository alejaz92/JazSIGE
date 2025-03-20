using Xunit;
using Moq;
using AuthService.Business.Interfaces;
using AuthService.Business.Models;
using AuthService.Business.Services;
using Microsoft.AspNetCore.Identity;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;

public class UserServiceTests
{
    private readonly Mock<IUserService> _userServiceMock;

    public UserServiceTests()
    {
        _userServiceMock = new Mock<IUserService>();
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnUsers()
    {
        // Arrange
        var users = new List<UserDTO>
        {
            new UserDTO { Id = 1, Username = "admin", FirstName = "Admin", LastName = "User", IsActive = true },
            new UserDTO { Id = 2, Username = "seller1", FirstName = "Juan", LastName = "Pérez", IsActive = true }
        };

        _userServiceMock.Setup(service => service.GetAllUsersAsync()).ReturnsAsync(users);

        // Act
        var result = await _userServiceMock.Object.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnSuccess_WhenUserIsCreated()
    {
        // Arrange
        var createUserDto = new CreateUserDTO { Username = "newUser", Password = "ValidPass123!" };

        _userServiceMock.Setup(service => service.CreateUserAsync(createUserDto))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userServiceMock.Object.CreateUserAsync(createUserDto);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnSuccess_WhenUserIsDeleted()
    {
        _userServiceMock.Setup(service => service.DeleteUserAsync(1))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _userServiceMock.Object.DeleteUserAsync(1);

        result.Succeeded.Should().BeTrue();
    }
}
