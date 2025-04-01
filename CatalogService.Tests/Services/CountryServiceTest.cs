using CatalogService.Business.Models.Country;
using CatalogService.Business.Services;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Moq;
using System.Linq.Expressions;


public class CountryServiceTests
{
    private readonly Mock<ICountryRepository> _repositoryMock = new();
    private readonly CountryService _service;

    public CountryServiceTests()
    {
        // Arrange: instanciar el servicio con mock del repositorio
        _service = new CountryService(_repositoryMock.Object);
    }

    private Country GetMockCountry()
    {
        return new Country
        {
            Id = 1,
            Name = "Argentina",
            IsActive = true
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnMappedDtos()
    {
        // Arrange
        var Countrys = new List<Country> { GetMockCountry() };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(Countrys);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Argentina", result.First().Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenCountryExists()
    {
        // Arrange
        var Country = GetMockCountry();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(Country);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Argentina", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenCountryDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Country?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var dto = new CountryCreateDTO { Name = "Nuevo Pais" };
        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Country, bool>>>())).ReturnsAsync(new List<Country>());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Country>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(GetMockCountry());

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Argentina", result.Name);
    }

    [Fact]
    public async Task CreateAsync_ThrowsException_WhenDescriptionNotUnique()
    {
        // Arrange
        var dto = new CountryCreateDTO { Name = "Argentina" };
        var Countrys = new List<Country> { GetMockCountry() };
        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Country, bool>>>())).ReturnsAsync(Countrys);

        // Act & Assert
        await Assert.ThrowsAsync<System.Exception>(() => _service.CreateAsync(dto));
    }


    [Fact]
    public async Task UpdateAsync_ReturnsDto_WhenExists()
    {
        // Arrange
        var Country = GetMockCountry();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(Country);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new CountryCreateDTO { Name = "Pais actualizado" };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Pais actualizado", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Country, object>>[]>())).ReturnsAsync((Country?)null);

        var dto = new CountryCreateDTO { Name = "Pais actualizado" };

        // Act
        var result = await _service.UpdateAsync(999, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenFound()
    {
        // Arrange
        var Country = GetMockCountry();
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(Country);
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
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Country?)null);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndReturnsTrue_WhenExists()
    {
        // Arrange
        var Country = GetMockCountry();
        Country.IsActive = false;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(Country);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.True(result);
        Assert.True(Country.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Country?)null);

        // Act
        var result = await _service.UpdateStatusAsync(999, true);

        // Assert
        Assert.False(result);
    }
}
