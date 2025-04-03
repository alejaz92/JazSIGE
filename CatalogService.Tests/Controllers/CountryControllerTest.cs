using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Brand;
using CatalogService.Business.Models.Country;
using CatalogService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CountryControllerTest
{
    private readonly Mock<ICountryService> _serviceMock;
    private readonly CountryController _controller;

    public CountryControllerTest()
    {
        // Arrange: configurar mocks y controller
        _serviceMock = new Mock<ICountryService>();
        _controller = new CountryController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithCountryList()
    {
        // Arrange
        var countries = new List<CountryDTO> { new CountryDTO { Id = 1, Name = "Argentina" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(countries);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCountries = Assert.IsAssignableFrom<IEnumerable<CountryDTO>>(okResult.Value);
        Assert.Single(returnedCountries);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult_WhenCountryExists()
    {
        // Arrange
        var Country = new CountryDTO { Id = 1, Name = "Argentina" };
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(Country);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCountry = Assert.IsType<CountryDTO>(okResult.Value);
        Assert.Equal(1, returnedCountry.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenCountryDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((CountryDTO)null);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsOkResult_WhenValid()
    {
        // Arrange
        var model = new CountryCreateDTO { Name = "Test" };
        var created = new CountryDTO { Id = 1, Name = "Test" };
        _serviceMock.Setup(s => s.CreateAsync(model)).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<CountryDTO>(okResult.Value);
        Assert.Equal("Test", returned.Name);

    }

    [Fact]
    public async Task Update_ReturnsOkResult_WhenFound()
    {
        // Arrange
        var model = new CountryCreateDTO { Name = "Test" };
        var updated = new CountryDTO { Id = 1, Name = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(1, model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<CountryDTO>(okResult.Value);
        Assert.Equal("Test", returned.Name);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var model = new CountryCreateDTO { Name = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(1, model)).ReturnsAsync((CountryDTO)null);

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

