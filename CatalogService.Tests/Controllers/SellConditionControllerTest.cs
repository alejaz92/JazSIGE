using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.SellCondition;
using CatalogService.Business.Models.Customer;
using CatalogService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class SellConditionControllerTests
{
    private readonly Mock<ISellConditionService> _serviceMock;
    private readonly SellConditionController _controller;

    public SellConditionControllerTests()
    {
        // Arrange: configurar mocks y controller
        _serviceMock = new Mock<ISellConditionService>();
        _controller = new SellConditionController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithSellConditionList()
    {
        // Arrange
        var SellConditions = new List<SellConditionDTO> { new SellConditionDTO { Id = 1, Description = "Mouse Logitech" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(SellConditions);

        // Act 
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSellConditions = Assert.IsAssignableFrom<IEnumerable<SellConditionDTO>>(okResult.Value);
        Assert.Single(returnedSellConditions);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult_WhenSellConditionsExists()
    {
        // Arrange
        var SellCondition = new SellConditionDTO { Id = 1, Description = "Mouse Logitech" };
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(SellCondition);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSellCondition = Assert.IsType<SellConditionDTO>(okResult.Value);
        Assert.Equal(1, returnedSellCondition.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenSellConditionDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((SellConditionDTO)null);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsOkResult_WhenValid()
    {
        // Arrange
        var model = new SellConditionCreateDTO { Description = "Test" };
        var created = new SellConditionDTO { Id = 1, Description = "Test" };
        _serviceMock.Setup(s => s.CreateAsync(model)).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<SellConditionDTO>(okResult.Value);
        Assert.Equal("Test", returned.Description);

    }

    [Fact]
    public async Task Update_ReturnsOkResult_WhenFound()
    {
        // Arrange
        var model = new SellConditionCreateDTO { Description = "Test" };
        var updated = new SellConditionDTO { Id = 1, Description = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(1, model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<SellConditionDTO>(okResult.Value);
        Assert.Equal("Test", returned.Description);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var model = new SellConditionCreateDTO { Description = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync((SellConditionDTO)null);

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

