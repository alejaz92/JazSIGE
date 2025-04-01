using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Customer;
using CatalogService.Controllers;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class CustomerControllerTests
{
    private readonly Mock<ICustomerService> _serviceMock;
    private readonly CustomerController _controller;

    public CustomerControllerTests()
    {
        // Arrange: configurar mocks y controller
        _serviceMock = new Mock<ICustomerService>();
        _controller = new CustomerController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithCustomerList()
    {
        // Arrange
        var customers = new List<CustomerDTO> { new CustomerDTO { Id = 1, CompanyName = "Test SA" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(customers);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCustomers = Assert.IsAssignableFrom<IEnumerable<CustomerDTO>>(okResult.Value);
        Assert.Single(returnedCustomers);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult_WhenCustomerExists()
    {
        // Arrange
        var customer = new CustomerDTO { Id = 1, CompanyName = "Test SA" };
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(customer);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCustomer = Assert.IsType<CustomerDTO>(okResult.Value);
        Assert.Equal(1, returnedCustomer.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((CustomerDTO)null);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsOkResult_WhenValid()
    {
        // Arrange
        var model = new CustomerCreateDTO { CompanyName = "New SA" };
        var created = new CustomerDTO { Id = 1, CompanyName = "New SA" };
        _serviceMock.Setup(s => s.CreateAsync(model)).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<CustomerDTO>(okResult.Value);
        Assert.Equal("New SA", returned.CompanyName);
    }

    [Fact]
    public async Task Update_ReturnsOkResult_WhenFound()
    {
        // Arrange
        var model = new CustomerCreateDTO { CompanyName = "Updated" };
        var updated = new CustomerDTO { Id = 1, CompanyName = "Updated" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(1, model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<CustomerDTO>(okResult.Value);
        Assert.Equal("Updated", returned.CompanyName);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var model = new CustomerCreateDTO { CompanyName = "Updated" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync((CustomerDTO)null);

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
