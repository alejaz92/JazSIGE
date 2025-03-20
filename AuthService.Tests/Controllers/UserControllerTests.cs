using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using AuthService.Business.Interfaces;
using AuthService.Controllers;
using AuthService.Business.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UserController _userController;

    public UserControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _userController = new UserController(_userServiceMock.Object);
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnOk_WithUserList()
    {
        //
        SetupUserContext(1);
        // Arrange
        var users = new List<UserDTO>
        {
            new UserDTO { Id = 1, Username = "admin", FirstName = "Admin", LastName = "User", IsActive = true },
            new UserDTO { Id = 2, Username = "seller1", FirstName = "Juan", LastName = "Pérez", IsActive = true }
        };

        _userServiceMock.Setup(service => service.GetAllUsersAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _userController.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUsers = Assert.IsType<List<UserDTO>>(okResult.Value);
        Assert.Equal(2, returnedUsers.Count);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnOk_WhenUserIsCreated()
    {
        SetupUserContext(1);
        // Arrange
        var createUserDto = new CreateUserDTO
        {
            Username = "newUser",
            Password = "ValidPass123!",
            FirstName = "John",
            LastName = "Doe",
            Role = "Seller"
        };

        _userServiceMock.Setup(service => service.CreateUserAsync(createUserDto))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userController.CreateUser(createUserDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenCreationFails()
    {
        SetupUserContext(1);
        // Arrange
        var createUserDto = new CreateUserDTO
        {
            Username = "newUser",
            Password = "ValidPass123!"
        };

        _userServiceMock.Setup(service => service.CreateUserAsync(createUserDto))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error creando usuario" }));

        // Act
        var result = await _userController.CreateUser(createUserDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    //[Fact]
    //public async Task DeleteUser_ShouldReturnOk_WhenUserIsDeleted()
    //{
    //    SetupUserContext(1);
    //    // Arrange
    //    _userServiceMock.Setup(service => service.DeleteUserAsync(1))
    //        .ReturnsAsync(IdentityResult.Success);

    //    // Act
    //    var result = await _userController.DeleteUser(1);

    //    // Assert
    //    var okResult = Assert.IsType<OkObjectResult>(result);
    //    Assert.Equal(200, okResult.StatusCode);
    //}

    [Fact]
    public async Task DeleteUser_ShouldReturnForbidden_WhenCalledByNonAdmin()
    {
        // Setup contexto de usuario común
        SetupUserContext(1);
        _userServiceMock.Setup(service => service.DeleteUserAsync(2))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Operación no autorizada" }));


        // Act
        var result = await _userController.DeleteUser(2);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Setup contexto de usuario como admin
        var userClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Role, "Admin")
    };
        var identity = new ClaimsIdentity(userClaims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _userController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Arrange
        _userServiceMock.Setup(service => service.DeleteUserAsync(99))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User not found" }));

        // Act
        var result = await _userController.DeleteUser(99);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnOk_WhenCalledByAdmin()
    {
        // Setup contexto de usuario como Admin
        var userClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Role, "Admin")
    };
        var identity = new ClaimsIdentity(userClaims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _userController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _userServiceMock.Setup(service => service.DeleteUserAsync(2))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userController.DeleteUser(2);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnOk_WhenCalledByAdmin()
    {
        // Setup contexto de usuario como Admin
        var userClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Role, "Admin")
    };
        var identity = new ClaimsIdentity(userClaims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _userController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Arrange
        var updateUserDto = new UpdateUserDTO
        {
            FirstName = "AdminUpdated",
            LastName = "AdminUser"
        };

        _userServiceMock.Setup(service => service.UpdateUserAsync(2, updateUserDto, true))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userController.UpdateUser(2, updateUserDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnOk_WhenCalledBySameUser()
    {
        // Setup contexto de usuario como el mismo usuario
        SetupUserContext(1);

        // Arrange
        var updateUserDto = new UpdateUserDTO
        {
            FirstName = "Updated",
            LastName = "User"
        };

        _userServiceMock.Setup(service => service.UpdateUserAsync(1, updateUserDto, false))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userController.UpdateUser(1, updateUserDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnForbidden_WhenCalledByDifferentUser()
    {
        // Setup contexto de usuario como otro usuario (ID 2)
        SetupUserContext(2);

        // Arrange
        var updateUserDto = new UpdateUserDTO
        {
            FirstName = "Updated",
            LastName = "User"
        };

        // Act
        var result = await _userController.UpdateUser(1, updateUserDto);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Setup contexto de usuario como admin
        var userClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Role, "Admin")
    };
        var identity = new ClaimsIdentity(userClaims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _userController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Arrange
        var updateUserDto = new UpdateUserDTO
        {
            FirstName = "Updated",
            LastName = "User"
        };

        _userServiceMock.Setup(service => service.UpdateUserAsync(99, updateUserDto, true))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User not found" }));

        // Act
        var result = await _userController.UpdateUser(99, updateUserDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task UpdateUserRole_ShouldReturnOk_WhenRoleIsUpdated()
    {
        SetupUserContext(1);
        // Arrange
        var updateUserRoleDto = new UpdateUserRoleDTO
        {
            Role = "Admin"
        };

        _userServiceMock.Setup(service => service.UpdateUserRoleAsync(1, "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userController.UpdateUserRole(1, updateUserRoleDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdateUserRole_ShouldReturnBadRequest_WhenRoleUpdateFails()
    {
        SetupUserContext(1);
        // Arrange
        var updateUserRoleDto = new UpdateUserRoleDTO
        {
            Role = "InvalidRole"
        };

        _userServiceMock.Setup(service => service.UpdateUserRoleAsync(1, "InvalidRole"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Rol inválido" }));

        // Act
        var result = await _userController.UpdateUserRole(1, updateUserRoleDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task UpdateUserStatus_ShouldReturnOk_WhenCalledByAdmin()
    {
        // Setup contexto de usuario como Admin
        var userClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Role, "Admin")
    };
        var identity = new ClaimsIdentity(userClaims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _userController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Arrange
        var updateUserStatusDto = new UpdateUserStatusDTO
        {
            IsActive = true
        };

        _userServiceMock.Setup(service => service.UpdateUserStatusAsync(2, true))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userController.UpdateUserStatus(2, updateUserStatusDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }


    [Fact]
    public async Task UpdateUserStatus_ShouldReturnForbidden_WhenCalledByNonAdmin()
    {
        // Setup contexto de usuario común
        SetupUserContext(1);

        // Arrange
        var updateUserStatusDto = new UpdateUserStatusDTO
        {
            IsActive = false
        };

        _userServiceMock.Setup(service => service.UpdateUserStatusAsync(2, false))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Operación no autorizada" }));


        // Act
        var result = await _userController.UpdateUserStatus(2, updateUserStatusDto);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
    }


    private void SetupUserContext(int userId)
    {
        var userClaims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(userClaims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _userController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
}
