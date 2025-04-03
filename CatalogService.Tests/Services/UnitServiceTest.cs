using CatalogService.Business.Models.Unit;
using CatalogService.Business.Services;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Moq;
using System.Linq.Expressions;


public class UnitServiceTests
{
    private readonly Mock<IUnitRepository> _repositoryMock = new();
    private readonly UnitService _service;

    public UnitServiceTests()
    {
        // Arrange: instanciar el servicio con mock del repositorio
        _service = new UnitService(_repositoryMock.Object);
    }

    private Unit GetMockUnit()
    {
        return new Unit
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
        var Units = new List<Unit> { GetMockUnit() };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(Units);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Logitech", result.First().Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenUnitExists()
    {
        // Arrange
        var Unit = GetMockUnit();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(Unit);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Logitech", result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUnitDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Unit?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var dto = new UnitCreateDTO { Description = "Nueva Marca" };
        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Unit, bool>>>())).ReturnsAsync(new List<Unit>());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Unit>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(GetMockUnit());

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
        var dto = new UnitCreateDTO { Description = "Logitech" };
        var Units = new List<Unit> { GetMockUnit() };
        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Unit, bool>>>())).ReturnsAsync(Units);

        // Act & Assert
        await Assert.ThrowsAsync<System.Exception>(() => _service.CreateAsync(dto));
    }


    [Fact]
    public async Task UpdateAsync_ReturnsDto_WhenExists()
    {
        // Arrange
        var Unit = GetMockUnit();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(Unit);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new UnitCreateDTO { Description = "Marca actualizada" };

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
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Unit, object>>[]>())).ReturnsAsync((Unit?)null);

        var dto = new UnitCreateDTO { Description = "Marca actualizada" };

        // Act
        var result = await _service.UpdateAsync(999, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenFound()
    {
        // Arrange
        var Unit = GetMockUnit();
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(Unit);
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
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Unit?)null);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndReturnsTrue_WhenExists()
    {
        // Arrange
        var Unit = GetMockUnit();
        Unit.IsActive = false;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(Unit);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.True(result);
        Assert.True(Unit.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Unit?)null);

        // Act
        var result = await _service.UpdateStatusAsync(999, true);

        // Assert
        Assert.False(result);
    }
}
