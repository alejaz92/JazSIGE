using CatalogService.Business.Models.Warehouse;
using CatalogService.Business.Services;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Moq;
using System.Linq.Expressions;


public class WarehouseServiceTests
{
    private readonly Mock<IWarehouseRepository> _repositoryMock = new();
    private readonly WarehouseService _service;

    public WarehouseServiceTests()
    {
        // Arrange: instanciar el servicio con mock del repositorio
        _service = new WarehouseService(_repositoryMock.Object);
    }

    private Warehouse GetMockWarehouse()
    {
        return new Warehouse
        {
            Id = 1,
            Description = "Logitech",
            IsActive = true
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnMappedDtos()
    {
        // Arrange
        var Warehouses = new List<Warehouse> { GetMockWarehouse() };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(Warehouses);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Logitech", result.First().Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenWarehouseExists()
    {
        // Arrange
        var Warehouse = GetMockWarehouse();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(Warehouse);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Logitech", result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenWarehouseDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Warehouse?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var dto = new WarehouseCreateDTO { Description = "Nueva Marca" };
        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Warehouse, bool>>>())).ReturnsAsync(new List<Warehouse>());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Warehouse>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(GetMockWarehouse());

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Logitech", result.Description);
    }

    [Fact]
    public async Task CreateAsync_ThrowsException_WhenDescriptionNotUnique()
    {
        // Arrange
        var dto = new WarehouseCreateDTO { Description = "Logitech" };
        var Warehouses = new List<Warehouse> { GetMockWarehouse() };
        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Warehouse, bool>>>())).ReturnsAsync(Warehouses);

        // Act & Assert
        await Assert.ThrowsAsync<System.Exception>(() => _service.CreateAsync(dto));
    }


    [Fact]
    public async Task UpdateAsync_ReturnsDto_WhenExists()
    {
        // Arrange
        var Warehouse = GetMockWarehouse();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(Warehouse);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new WarehouseCreateDTO { Description = "Marca actualizada" };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Marca actualizada", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Warehouse, object>>[]>())).ReturnsAsync((Warehouse?)null);

        var dto = new WarehouseCreateDTO { Description = "Marca actualizada" };

        // Act
        var result = await _service.UpdateAsync(999, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenFound()
    {
        // Arrange
        var Warehouse = GetMockWarehouse();
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(Warehouse);
        _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Warehouse?)null);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndReturnsTrue_WhenExists()
    {
        // Arrange
        var Warehouse = GetMockWarehouse();
        Warehouse.IsActive = false;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(Warehouse);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.True(result);
        Assert.True(Warehouse.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Warehouse?)null);

        // Act
        var result = await _service.UpdateStatusAsync(999, true);

        // Assert
        Assert.False(result);
    }
}
