using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StockService.Business.Interfaces;
using StockService.Business.Models;
using StockService.Controllers;
using Xunit;
using FluentAssertions;

namespace StockService.Tests.Controllers;

public class StockControllerTests
{
    private readonly Mock<IStockService> _stockServiceMock = new();
    private readonly Mock<IEnumService> _enumServiceMock = new();
    private readonly StockController _controller;

    public StockControllerTests()
    {
        _controller = new StockController(_stockServiceMock.Object, _enumServiceMock.Object);
    }

    [Fact]
    public async Task RegisterMovement_Should_Return_Unauthorized_When_UserId_Header_Is_Missing()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var dto = new StockMovementCreateDTO();

        // Act
        var result = await _controller.RegisterMovement(dto);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task RegisterMovement_Should_Return_BadRequest_On_ArgumentException()
    {
        // Arrange
        var dto = new StockMovementCreateDTO { ArticleId = 1 };
        var context = new DefaultHttpContext();
        context.Request.Headers["X-UserId"] = "123";

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };

        _stockServiceMock.Setup(s => s.RegisterMovementAsync(dto, 123))
                         .ThrowsAsync(new ArgumentException("Invalid article"));

        // Act
        var result = await _controller.RegisterMovement(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { error = "Invalid article" });
    }

    [Fact]
    public async Task RegisterMovement_Should_Return_Ok_When_Successful()
    {
        // Arrange
        var dto = new StockMovementCreateDTO { ArticleId = 1 };
        var context = new DefaultHttpContext();
        context.Request.Headers["X-UserId"] = "123";

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };

        // Act
        var result = await _controller.RegisterMovement(dto);

        // Assert
        result.Should().BeOfType<OkResult>();
        _stockServiceMock.Verify(s => s.RegisterMovementAsync(dto, 123), Times.Once);
    }

    [Fact]
    public async Task GetStock_Should_Return_Ok_With_Quantity()
    {
        // Arrange
        int articleId = 1;
        int warehouseId = 2;
        decimal expectedQuantity = 15;

        _stockServiceMock.Setup(s => s.GetStockAsync(articleId, warehouseId))
                         .ReturnsAsync(expectedQuantity);

        // Act
        var result = await _controller.GetStock(articleId, warehouseId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(expectedQuantity);
    }

    [Fact]
    public async Task GetStock_Should_Return_500_On_Exception()
    {
        // Arrange
        _stockServiceMock.Setup(s => s.GetStockAsync(It.IsAny<int>(), It.IsAny<int>()))
                         .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetStock(1, 2);

        // Assert
        var error = result.Result as ObjectResult;
        error!.StatusCode.Should().Be(500);
        error.Value.Should().BeEquivalentTo(new { error = "Unexpected error" });
    }

    [Fact]
    public async Task GetStockSummary_Should_Return_Total_Stock()
    {
        // Arrange
        int articleId = 5;
        decimal total = 40;

        _stockServiceMock.Setup(s => s.GetStockSummaryAsync(articleId))
                         .ReturnsAsync(total);

        // Act
        var result = await _controller.GetStockSummary(articleId);

        // Assert
        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().Be(total);
    }

    [Fact]
    public async Task GetMovementsByArticle_Should_Return_Paginated_List()
    {
        // Arrange
        int articleId = 10;
        int page = 1, pageSize = 2;
        var mockList = new List<StockMovementDTO>
    {
        new() { Id = 1, Quantity = 10 },
        new() { Id = 2, Quantity = 5 }
    };

        _stockServiceMock.Setup(s => s.GetMovementsByArticleAsync(articleId, page, pageSize))
                         .ReturnsAsync(mockList);

        // Act
        var result = await _controller.GetMovementsByArticle(articleId, page, pageSize);

        // Assert
        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().BeEquivalentTo(mockList);
    }

    [Fact]
    public void GetMovementTypes_Should_Return_EnumDTO_List()
    {
        // Arrange
        var enumList = new List<EnumDTO>
    {
        new() { Name = "Purchase", Value = 0 },
        new() { Name = "Sale", Value = 1 }
    };

        _enumServiceMock.Setup(e => e.GetStockMovementTypes()).Returns(enumList);

        // Act
        var result = _controller.GetMovementTypes();

        // Assert
        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().BeEquivalentTo(enumList);
    }
}
