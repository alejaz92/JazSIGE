using CatalogService.Business.Models.GrossIncomeType;
using CatalogService.Business.Services;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Moq;
using System.Linq.Expressions;


public class GrossIncomeTypeServiceTests
{
    private readonly Mock<IGrossIncomeTypeRepository> _repositoryMock = new();
    private readonly GrossIncomeTypeService _service;

    public GrossIncomeTypeServiceTests()
    {
        // Arrange: instanciar el servicio con mock del repositorio
        _service = new GrossIncomeTypeService(_repositoryMock.Object);
    }

    private GrossIncomeType GetMockGrossIncomeType()
    {
        return new GrossIncomeType
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
        var GrossIncomeTypes = new List<GrossIncomeType> { GetMockGrossIncomeType() };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(GrossIncomeTypes);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Logitech", result.First().Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenGrossIncomeTypeExists()
    {
        // Arrange
        var GrossIncomeType = GetMockGrossIncomeType();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(GrossIncomeType);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Logitech", result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenGrossIncomeTypeDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((GrossIncomeType?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var dto = new GrossIncomeTypeCreateDTO { Description = "Nueva Marca" };
        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<GrossIncomeType, bool>>>())).ReturnsAsync(new List<GrossIncomeType>());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<GrossIncomeType>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(GetMockGrossIncomeType());

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
        var dto = new GrossIncomeTypeCreateDTO { Description = "Logitech" };
        var GrossIncomeTypes = new List<GrossIncomeType> { GetMockGrossIncomeType() };
        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<GrossIncomeType, bool>>>())).ReturnsAsync(GrossIncomeTypes);

        // Act & Assert
        await Assert.ThrowsAsync<System.Exception>(() => _service.CreateAsync(dto));
    }


    [Fact]
    public async Task UpdateAsync_ReturnsDto_WhenExists()
    {
        // Arrange
        var GrossIncomeType = GetMockGrossIncomeType();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(GrossIncomeType);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new GrossIncomeTypeCreateDTO { Description = "Marca actualizada" };

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
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<GrossIncomeType, object>>[]>())).ReturnsAsync((GrossIncomeType?)null);

        var dto = new GrossIncomeTypeCreateDTO { Description = "Marca actualizada" };

        // Act
        var result = await _service.UpdateAsync(999, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenFound()
    {
        // Arrange
        var GrossIncomeType = GetMockGrossIncomeType();
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(GrossIncomeType);
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
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GrossIncomeType?)null);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndReturnsTrue_WhenExists()
    {
        // Arrange
        var GrossIncomeType = GetMockGrossIncomeType();
        GrossIncomeType.IsActive = false;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(GrossIncomeType);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.True(result);
        Assert.True(GrossIncomeType.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GrossIncomeType?)null);

        // Act
        var result = await _service.UpdateStatusAsync(999, true);

        // Assert
        Assert.False(result);
    }
}
