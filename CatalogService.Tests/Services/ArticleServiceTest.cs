using CatalogService.Business.Models.Article;
using CatalogService.Business.Services;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Moq;
using System.Linq.Expressions;


public class ArticleServiceTests
{
    private readonly Mock<IArticleRepository> _repositoryMock = new();
    private readonly ArticleService _service;

    public ArticleServiceTests()
    {
        // Arrange: instanciar el servicio con mock del repositorio
        _service = new ArticleService(_repositoryMock.Object);
    }

    private Article GetMockArticle()
    {
        return new Article
        {
            Id = 1,
            Description = "Mouse Logitech",
            SKU = "LOG-MOUSE-001",
            BrandId = 1,
            Brand = new Brand { Description = "Logitech" },
            LineId = 1,
            Line = new Line { Description = "Periféricos" },
            UnitId = 1,
            Unit = new Unit { Description = "Unidad" },
            IsTaxed = true,
            IVAPercentage = 21,
            GrossIncomeTypeId = 1,
            GrossIncomeType = new GrossIncomeType { Description = "General" },
            Warranty = 12,
            IsVisible = true
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnMappedDtos()
    {
        // Arrange
        var articles = new List<Article> { GetMockArticle() };
        _repositoryMock.Setup(r => r.GetAllIncludingAsync(It.IsAny<Expression<Func<Article, object>>[]>()))
            .ReturnsAsync(articles);   

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Mouse Logitech", result.First().Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenArticleExists()
    {
        // Arrange
        var article = GetMockArticle();
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Article, object>>[]>()))
            .ReturnsAsync(article);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mouse Logitech", result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenArticleDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Article, object>>[]>()))
            .ReturnsAsync((Article?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var dto = new ArticleCreateDTO { Description = "Nuevo artículo", BrandId = 1, LineId = 1, UnitId = 1, IsTaxed = true, IVAPercentage = 21, GrossIncomeTypeId = 1, Warranty = 6, IsVisible = true };
        var article = GetMockArticle();

        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Article, bool>>>())).ReturnsAsync(new List<Article>());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Article>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Article, object>>[]>())).ReturnsAsync(article);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mouse Logitech", result.Description);
    }

    [Fact]
    public async Task CreateAsync_ThrowsException_WhenDescriptionNotUnique()
    {
        // Arrange
        var dto = new ArticleCreateDTO { Description = "Mouse Logitech" };
        var articles = new List<Article> { GetMockArticle() };
        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Article, bool>>>())).ReturnsAsync(articles);

        // Act & Assert
        await Assert.ThrowsAsync<System.Exception>(() => _service.CreateAsync(dto));
    }
    

    [Fact]
    public async Task UpdateAsync_ReturnsDto_WhenExists()
    {
        // Arrange
        var article = GetMockArticle();
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Article, object>>[]>())).ReturnsAsync(article);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new ArticleCreateDTO { Description = "Artículo Actualizado" };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Artículo Actualizado", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Article, object>>[]>())).ReturnsAsync((Article?)null);

        var dto = new ArticleCreateDTO { Description = "Artículo Actualizado" };

        // Act
        var result = await _service.UpdateAsync(999, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenFound()
    {
        // Arrange
        var article = GetMockArticle();
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(article);
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
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Article?)null);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndReturnsTrue_WhenExists()
    {
        // Arrange
        var article = GetMockArticle();
        article.IsActive = false;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(article);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.True(result);
        Assert.True(article.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Article?)null);

        // Act
        var result = await _service.UpdateStatusAsync(999, true);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateVisibilityAsync_TogglesVisibility_WhenExists()
    {
        // Arrange
        var article = GetMockArticle();
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Article, object>>[]>())).ReturnsAsync(article);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateVisibilityAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsVisible); // se invierte
    }

    [Fact]
    public async Task UpdateVisibilityAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Article, object>>[]>())).ReturnsAsync((Article?)null);

        // Act
        var result = await _service.UpdateVisibilityAsync(999);

        // Assert
        Assert.Null(result);
    }

}
