using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Transport;
using CatalogService.Business.Models.Customer;
using CatalogService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class TransportControllerTests
{
    private readonly Mock<ITransportService> _serviceMock;
    private readonly TransportController _controller;

    public TransportControllerTests()
    {
        // Arrange: configurar mocks y controller
        _serviceMock = new Mock<ITransportService>();
        _controller = new TransportController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithTransportList()
    {
        // Arrange
        var Transports = new List<TransportDTO> { new TransportDTO { Id = 1, Name = "Mouse Logitech" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(Transports);

        // Act 
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTransports = Assert.IsAssignableFrom<IEnumerable<TransportDTO>>(okResult.Value);
        Assert.Single(returnedTransports);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult_WhenTransportsExists()
    {
        // Arrange
        var Transport = new TransportDTO { Id = 1, Name = "Mouse Logitech" };
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(Transport);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTransport = Assert.IsType<TransportDTO>(okResult.Value);
        Assert.Equal(1, returnedTransport.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenTransportDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((TransportDTO)null);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsOkResult_WhenValid()
    {
        // Arrange
        var model = new TransportCreateDTO { Name = "Test" };
        var created = new TransportDTO { Id = 1, Name = "Test" };
        _serviceMock.Setup(s => s.CreateAsync(model)).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<TransportDTO>(okResult.Value);
        Assert.Equal("Test", returned.Name);

    }

    [Fact]
    public async Task Update_ReturnsOkResult_WhenFound()
    {
        // Arrange
        var model = new TransportCreateDTO { Name = "Test" };
        var updated = new TransportDTO { Id = 1, Name = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(1, model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<TransportDTO>(okResult.Value);
        Assert.Equal("Test", returned.Name);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var model = new TransportCreateDTO { Name = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync((TransportDTO)null);

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


}

