using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Line;
using CatalogService.Business.Models.Customer;
using CatalogService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class LineControllerTests
{
    private readonly Mock<ILineService> _serviceMock;
    private readonly LineController _controller;

    public LineControllerTests()
    {
        // Arrange: configurar mocks y controller
        _serviceMock = new Mock<ILineService>();
        _controller = new LineController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithLineList()
    {
        // Arrange
        var Lines = new List<LineDTO> { new LineDTO { Id = 1, Description = "Mouse Logitech" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(Lines);

        // Act 
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedLines = Assert.IsAssignableFrom<IEnumerable<LineDTO>>(okResult.Value);
        Assert.Single(returnedLines);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult_WhenLinesExists()
    {
        // Arrange
        var Line = new LineDTO { Id = 1, Description = "Mouse Logitech" };
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(Line);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedLine = Assert.IsType<LineDTO>(okResult.Value);
        Assert.Equal(1, returnedLine.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenLineDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((LineDTO)null);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsOkResult_WhenValid()
    {
        // Arrange
        var model = new LineCreateDTO { Description = "Test" };
        var created = new LineDTO { Id = 1, Description = "Test" };
        _serviceMock.Setup(s => s.CreateAsync(model)).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<LineDTO>(okResult.Value);
        Assert.Equal("Test", returned.Description);

    }

    [Fact]
    public async Task Update_ReturnsOkResult_WhenFound()
    {
        // Arrange
        var model = new LineCreateDTO { Description = "Test" };
        var updated = new LineDTO { Id = 1, Description = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(1, model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<LineDTO>(okResult.Value);
        Assert.Equal("Test", returned.Description);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var model = new LineCreateDTO { Description = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync((LineDTO)null);

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

