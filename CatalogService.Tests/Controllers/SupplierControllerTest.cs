using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Supplier;
using CatalogService.Business.Models.Customer;
using CatalogService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class SupplierControllerTests
{
    private readonly Mock<ISupplierService> _serviceMock;
    private readonly SupplierController _controller;

    public SupplierControllerTests()
    {
        // Arrange: configurar mocks y controller
        _serviceMock = new Mock<ISupplierService>();
        _controller = new SupplierController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithSupplierList()
    {
        // Arrange
        var Suppliers = new List<SupplierDTO> { new SupplierDTO { Id = 1, CompanyName = "Mouse Logitech" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(Suppliers);

        // Act 
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSuppliers = Assert.IsAssignableFrom<IEnumerable<SupplierDTO>>(okResult.Value);
        Assert.Single(returnedSuppliers);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult_WhenSuppliersExists()
    {
        // Arrange
        var Supplier = new SupplierDTO { Id = 1, CompanyName = "Mouse Logitech" };
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(Supplier);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSupplier = Assert.IsType<SupplierDTO>(okResult.Value);
        Assert.Equal(1, returnedSupplier.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenSupplierDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((SupplierDTO)null);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsOkResult_WhenValid()
    {
        // Arrange
        var model = new SupplierCreateDTO { CompanyName = "Test" };
        var created = new SupplierDTO { Id = 1, CompanyName = "Test" };
        _serviceMock.Setup(s => s.CreateAsync(model)).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<SupplierDTO>(okResult.Value);
        Assert.Equal("Test", returned.CompanyName);

    }

    [Fact]
    public async Task Update_ReturnsOkResult_WhenFound()
    {
        // Arrange
        var model = new SupplierCreateDTO { CompanyName = "Test" };
        var updated = new SupplierDTO { Id = 1, CompanyName = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(1, model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<SupplierDTO>(okResult.Value);
        Assert.Equal("Test", returned.CompanyName);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var model = new SupplierCreateDTO { CompanyName = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync((SupplierDTO)null);

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

