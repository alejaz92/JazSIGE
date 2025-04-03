using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.GrossIncomeType;
using CatalogService.Business.Models.Customer;
using CatalogService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class GrossIncomeTypeControllerTests
{
    private readonly Mock<IGrossIncomeTypeService> _serviceMock;
    private readonly GrossIncomeTypeController _controller;

    public GrossIncomeTypeControllerTests()
    {
        // Arrange: configurar mocks y controller
        _serviceMock = new Mock<IGrossIncomeTypeService>();
        _controller = new GrossIncomeTypeController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithGrossIncomeTypeList()
    {
        // Arrange
        var GrossIncomeTypes = new List<GrossIncomeTypeDTO> { new GrossIncomeTypeDTO { Id = 1, Description = "Mouse Logitech" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(GrossIncomeTypes);

        // Act 
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGrossIncomeTypes = Assert.IsAssignableFrom<IEnumerable<GrossIncomeTypeDTO>>(okResult.Value);
        Assert.Single(returnedGrossIncomeTypes);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult_WhenGrossIncomeTypesExists()
    {
        // Arrange
        var GrossIncomeType = new GrossIncomeTypeDTO { Id = 1, Description = "Mouse Logitech" };
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(GrossIncomeType);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGrossIncomeType = Assert.IsType<GrossIncomeTypeDTO>(okResult.Value);
        Assert.Equal(1, returnedGrossIncomeType.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenGrossIncomeTypeDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((GrossIncomeTypeDTO)null);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsOkResult_WhenValid()
    {
        // Arrange
        var model = new GrossIncomeTypeCreateDTO { Description = "Test" };
        var created = new GrossIncomeTypeDTO { Id = 1, Description = "Test" };
        _serviceMock.Setup(s => s.CreateAsync(model)).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<GrossIncomeTypeDTO>(okResult.Value);
        Assert.Equal("Test", returned.Description);

    }

    [Fact]
    public async Task Update_ReturnsOkResult_WhenFound()
    {
        // Arrange
        var model = new GrossIncomeTypeCreateDTO { Description = "Test" };
        var updated = new GrossIncomeTypeDTO { Id = 1, Description = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(1, model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<GrossIncomeTypeDTO>(okResult.Value);
        Assert.Equal("Test", returned.Description);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var model = new GrossIncomeTypeCreateDTO { Description = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync((GrossIncomeTypeDTO)null);

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

