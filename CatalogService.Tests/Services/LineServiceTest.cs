using CatalogService.Business.Models.City;
using CatalogService.Business.Models.Line;
using CatalogService.Business.Services;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Moq;
using System.Linq.Expressions;

public class LineServiceTests
{
    private readonly Mock<ILineRepository> _repositoryMock = new();
    private readonly LineService _service;

    public LineServiceTests()
    {
        // Arrange: creamos el servicio usando el mock del repositorio
        _service = new LineService(_repositoryMock.Object);
    }

    private Line GetMockLine()
    {
        return new Line
        {
            Id = 1,
            Description = "Cables HDMI",
            LineGroupId = 1,
            LineGroup = new LineGroup
            {
                Description = "Cables"
            },
            IsActive = true
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnMappedDtos()
    {
        // Arrange
        var lines = new List<Line> { GetMockLine() };
        _repositoryMock.Setup(r => r.GetAllIncludingAsync(It.IsAny<Expression<Func<Line, object>>[]>()))
                       .ReturnsAsync(lines);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Cables HDMI", result.First().Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenLineExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Line, object>>[]>()))
                       .ReturnsAsync(GetMockLine());

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Cables HDMI", result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenLineDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Line, object>>[]>()))
                       .ReturnsAsync((Line?)null);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var dto = new LineCreateDTO { Description = "Nueva linea", LineGroupId = 1 };

        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Line, bool>>>()))
                       .ReturnsAsync(new List<Line>());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Line>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Line, object>>[]>()))
                       .ReturnsAsync(GetMockLine());

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Cables HDMI", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsDto_WhenExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Line, object>>[]>()))
                       .ReturnsAsync(GetMockLine());
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new LineCreateDTO { Description = "Linea modificada", LineGroupId = 1 };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Linea modificada", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Line, object>>[]>()))
                       .ReturnsAsync((Line?)null);

        var dto = new LineCreateDTO { Description = "Linea modificada", LineGroupId = 1 };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenLineExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                       .ReturnsAsync(GetMockLine());
        _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenLineNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Line?)null);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndReturnsTrue_WhenLineExists()
    {
        // Arrange
        var line = GetMockLine();
        line.IsActive = false;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(line);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.True(result);
        Assert.True(line.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenLineNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Line?)null);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.False(result);
    }
}