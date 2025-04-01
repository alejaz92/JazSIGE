using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Article;
using CatalogService.Business.Models.Customer;
using CatalogService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class ArticleControllerTests
{
    private readonly Mock<IArticleService> _serviceMock;
    private readonly ArticleController _controller;

    public ArticleControllerTests()
    {
        // Arrange: configurar mocks y controller
        _serviceMock = new Mock<IArticleService>();
        _controller = new ArticleController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithArticleList()
    {
        // Arrange
        var articles = new List<ArticleDTO> { new ArticleDTO { Id = 1, Description = "Mouse Logitech" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(articles);

        // Act 
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedArticles = Assert.IsAssignableFrom<IEnumerable<ArticleDTO>>(okResult.Value);
        Assert.Single(returnedArticles);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult_WhenArticlesExists()
    {
        // Arrange
        var article = new ArticleDTO { Id = 1, Description = "Mouse Logitech" };
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(article);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedArticle = Assert.IsType<ArticleDTO>(okResult.Value);
        Assert.Equal(1, returnedArticle.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenArticleDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((ArticleDTO)null);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsOkResult_WhenValid()
    {
        // Arrange
        var model = new ArticleCreateDTO { Description = "Test" };
        var created = new ArticleDTO { Id = 1, Description = "Test" };
        _serviceMock.Setup(s => s.CreateAsync(model)).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<ArticleDTO>(okResult.Value);
        Assert.Equal("Test", returned.Description);

    }

    [Fact]
    public async Task Update_ReturnsOkResult_WhenFound()
    {
        // Arrange
        var model = new ArticleCreateDTO { Description = "Test" };
        var updated = new ArticleDTO { Id = 1, Description = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(1, model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<ArticleDTO>(okResult.Value);
        Assert.Equal("Test", returned.Description);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var model = new ArticleCreateDTO { Description = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync((ArticleDTO)null);

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

    [Fact]
    public async Task ToggleVisibility_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var updated = new ArticleDTO { Id = 1, Description = "Artículo", IsVisible = false };
        _serviceMock.Setup(s => s.UpdateVisibilityAsync(1)).ReturnsAsync(updated);

        // Act
        var result = await _controller.UpdateVisibility(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<ArticleDTO>(okResult.Value);
        Assert.False(returned.IsVisible);
    }

    [Fact]
    public async Task ToggleVisibility_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        _serviceMock.Setup(s => s.UpdateVisibilityAsync(1)).ReturnsAsync((ArticleDTO)null);

        // Act
        var result = await _controller.UpdateVisibility(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}

