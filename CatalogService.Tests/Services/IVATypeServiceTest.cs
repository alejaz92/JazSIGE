using CatalogService.Business.Models.IVAType;
using CatalogService.Business.Services;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Moq;
using System.Linq.Expressions;


public class IVATypeServiceTests
{
    private readonly Mock<IIVATypeRepository> _repositoryMock = new();
    private readonly IVATypeService _service;

    public IVATypeServiceTests()
    {
        // Arrange: instanciar el servicio con mock del repositorio
        _service = new IVATypeService(_repositoryMock.Object);
    }

    private IVAType GetMockIVAType()
    {
        return new IVAType
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
        var IVATypes = new List<IVAType> { GetMockIVAType() };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(IVATypes);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Logitech", result.First().Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenIVATypeExists()
    {
        // Arrange
        var IVAType = GetMockIVAType();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(IVAType);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Logitech", result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenIVATypeDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((IVAType?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var dto = new IVATypeCreateDTO { Description = "Nueva Marca" };
        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<IVAType, bool>>>())).ReturnsAsync(new List<IVAType>());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<IVAType>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(GetMockIVAType());

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
        var dto = new IVATypeCreateDTO { Description = "Logitech" };
        var IVATypes = new List<IVAType> { GetMockIVAType() };
        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<IVAType, bool>>>())).ReturnsAsync(IVATypes);

        // Act & Assert
        await Assert.ThrowsAsync<System.Exception>(() => _service.CreateAsync(dto));
    }


    [Fact]
    public async Task UpdateAsync_ReturnsDto_WhenExists()
    {
        // Arrange
        var IVAType = GetMockIVAType();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(IVAType);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new IVATypeCreateDTO { Description = "Marca actualizada" };

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
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<IVAType, object>>[]>())).ReturnsAsync((IVAType?)null);

        var dto = new IVATypeCreateDTO { Description = "Marca actualizada" };

        // Act
        var result = await _service.UpdateAsync(999, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenFound()
    {
        // Arrange
        var IVAType = GetMockIVAType();
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(IVAType);
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
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((IVAType?)null);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndReturnsTrue_WhenExists()
    {
        // Arrange
        var IVAType = GetMockIVAType();
        IVAType.IsActive = false;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(IVAType);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.True(result);
        Assert.True(IVAType.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((IVAType?)null);

        // Act
        var result = await _service.UpdateStatusAsync(999, true);

        // Assert
        Assert.False(result);
    }
}
