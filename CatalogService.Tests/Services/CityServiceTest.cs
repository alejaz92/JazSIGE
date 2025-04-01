// CityServiceTests.cs - con comentarios AAA (Arrange, Act, Assert)

using CatalogService.Business.Models.City;
using CatalogService.Business.Services;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

public class CityServiceTests
{
    private readonly Mock<ICityRepository> _repositoryMock = new();
    private readonly CityService _service;

    public CityServiceTests()
    {
        // Arrange: creamos el servicio usando el mock del repositorio
        _service = new CityService(_repositoryMock.Object);
    }

    private City GetMockCity()
    {
        return new City
        {
            Id = 1,
            Name = "Rosario",
            ProvinceId = 1,
            Province = new Province
            {
                Name = "Santa Fe",
                Country = new Country { Name = "Argentina" }
            },
            IsActive = true
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnMappedDtos()
    {
        // Arrange
        var cities = new List<City> { GetMockCity() };
        _repositoryMock.Setup(r => r.GetAllIncludingAsync(It.IsAny<Expression<Func<City, object>>[]>()))
                       .ReturnsAsync(cities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Rosario", result.First().Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenCityExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<City, object>>[]>()))
                       .ReturnsAsync(GetMockCity());

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Rosario", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenCityDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<City, object>>[]>()))
                       .ReturnsAsync((City?)null);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var dto = new CityCreateDTO { Name = "Nueva ciudad", ProvinceId = 1 };

        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<City, bool>>>()))
                       .ReturnsAsync(new List<City>());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<City>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<City, object>>[]>()))
                       .ReturnsAsync(GetMockCity());

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Rosario", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsDto_WhenExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<City, object>>[]>()))
                       .ReturnsAsync(GetMockCity());
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new CityCreateDTO { Name = "Ciudad modificada", ProvinceId = 1 };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Ciudad modificada", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<City, object>>[]>()))
                       .ReturnsAsync((City?)null);

        var dto = new CityCreateDTO { Name = "Ciudad modificada", ProvinceId = 1 };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenCityExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                       .ReturnsAsync(GetMockCity());
        _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenCityNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((City?)null);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndReturnsTrue_WhenCityExists()
    {
        // Arrange
        var city = GetMockCity();
        city.IsActive = false;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(city);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.True(result);
        Assert.True(city.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenCityNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((City?)null);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.False(result);
    }
}
