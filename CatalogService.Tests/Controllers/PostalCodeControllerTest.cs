using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.PostalCode;
using CatalogService.Business.Models.Customer;
using CatalogService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class PostalCodeControllerTests
{
    private readonly Mock<IPostalCodeService> _serviceMock;
    private readonly PostalCodeController _controller;

    public PostalCodeControllerTests()
    {
        // Arrange: configurar mocks y controller
        _serviceMock = new Mock<IPostalCodeService>();
        _controller = new PostalCodeController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithPostalCodeList()
    {
        // Arrange
        var PostalCodes = new List<PostalCodeDTO> { new PostalCodeDTO { Id = 1, Code = "Mouse Logitech" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(PostalCodes);

        // Act 
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPostalCodes = Assert.IsAssignableFrom<IEnumerable<PostalCodeDTO>>(okResult.Value);
        Assert.Single(returnedPostalCodes);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult_WhenPostalCodesExists()
    {
        // Arrange
        var PostalCode = new PostalCodeDTO { Id = 1, Code = "Mouse Logitech" };
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(PostalCode);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPostalCode = Assert.IsType<PostalCodeDTO>(okResult.Value);
        Assert.Equal(1, returnedPostalCode.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenPostalCodeDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((PostalCodeDTO)null);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsOkResult_WhenValid()
    {
        // Arrange
        var model = new PostalCodeCreateDTO { Code = "Test" };
        var created = new PostalCodeDTO { Id = 1, Code = "Test" };
        _serviceMock.Setup(s => s.CreateAsync(model)).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<PostalCodeDTO>(okResult.Value);
        Assert.Equal("Test", returned.Code);

    }

    [Fact]
    public async Task Update_ReturnsOkResult_WhenFound()
    {
        // Arrange
        var model = new PostalCodeCreateDTO { Code = "Test" };
        var updated = new PostalCodeDTO { Id = 1, Code = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(1, model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<PostalCodeDTO>(okResult.Value);
        Assert.Equal("Test", returned.Code);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var model = new PostalCodeCreateDTO { Code = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync((PostalCodeDTO)null);

        // Act
        var result = await _controller.Update(1, model);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsNoContent_WhenSuccess()
    {
        // Arrange
        _serviceMock.Setup(s => s.UpdateStatusAsync(1, true)).ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateStatus(1, true);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsNotFound_WhenCustomerMissing()
    {
        // Arrange
        _serviceMock.Setup(s => s.UpdateStatusAsync(1, true)).ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateStatus(1, true);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }


}

