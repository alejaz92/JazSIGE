using CatalogService.Business.Models.PriceList;
using CatalogService.Business.Services;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Moq;
using System.Linq.Expressions;


public class PriceListServiceTests
{
    private readonly Mock<IPriceListRepository> _repositoryMock = new();
    private readonly PriceListService _service;

    public PriceListServiceTests()
    {
        // Arrange: instanciar el servicio con mock del repositorio
        _service = new PriceListService(_repositoryMock.Object);
    }

    private PriceList GetMockPriceList()
    {
        return new PriceList
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
        var PriceLists = new List<PriceList> { GetMockPriceList() };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(PriceLists);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Logitech", result.First().Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenPriceListExists()
    {
        // Arrange
        var PriceList = GetMockPriceList();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PriceList);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Logitech", result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenPriceListDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((PriceList?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var dto = new PriceListCreateDTO { Description = "Nueva Marca" };
        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PriceList, bool>>>())).ReturnsAsync(new List<PriceList>());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<PriceList>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(GetMockPriceList());

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
        var dto = new PriceListCreateDTO { Description = "Logitech" };
        var PriceLists = new List<PriceList> { GetMockPriceList() };
        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PriceList, bool>>>())).ReturnsAsync(PriceLists);

        // Act & Assert
        await Assert.ThrowsAsync<System.Exception>(() => _service.CreateAsync(dto));
    }


    [Fact]
    public async Task UpdateAsync_ReturnsDto_WhenExists()
    {
        // Arrange
        var PriceList = GetMockPriceList();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(PriceList);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new PriceListCreateDTO { Description = "Marca actualizada" };

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
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<PriceList, object>>[]>())).ReturnsAsync((PriceList?)null);

        var dto = new PriceListCreateDTO { Description = "Marca actualizada" };

        // Act
        var result = await _service.UpdateAsync(999, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenFound()
    {
        // Arrange
        var PriceList = GetMockPriceList();
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(PriceList);
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
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((PriceList?)null);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndReturnsTrue_WhenExists()
    {
        // Arrange
        var PriceList = GetMockPriceList();
        PriceList.IsActive = false;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(PriceList);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.True(result);
        Assert.True(PriceList.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((PriceList?)null);

        // Act
        var result = await _service.UpdateStatusAsync(999, true);

        // Assert
        Assert.False(result);
    }
}
