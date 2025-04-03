// ProvinceServiceTests.cs - con comentarios AAA (Arrange, Act, Assert)

using CatalogService.Business.Models.Province;
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

public class ProvinceServiceTests
{
    private readonly Mock<IProvinceRepository> _repositoryMock = new();
    private readonly ProvinceService _service;

    public ProvinceServiceTests()
    {
        // Arrange: creamos el servicio usando el mock del repositorio
        _service = new ProvinceService(_repositoryMock.Object);
    }

    private Province GetMockProvince()
    {
        return new Province
        {
            Id = 1,
            Name = "Rosario",
            CountryId = 1,
            Country = new Country
            {
                Name = "Argentina"
            },
            IsActive = true
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnMappedDtos()
    {
        // Arrange
        var cities = new List<Province> { GetMockProvince() };
        _repositoryMock.Setup(r => r.GetAllIncludingAsync(It.IsAny<Expression<Func<Province, object>>[]>()))
                       .ReturnsAsync(cities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Rosario", result.First().Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenProvinceExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Province, object>>[]>()))
                       .ReturnsAsync(GetMockProvince());

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Rosario", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenProvinceDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Province, object>>[]>()))
                       .ReturnsAsync((Province?)null);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var dto = new ProvinceCreateDTO { Name = "Nueva ciudad", CountryId = 1 };

        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Province, bool>>>()))
                       .ReturnsAsync(new List<Province>());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Province>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Province, object>>[]>()))
                       .ReturnsAsync(GetMockProvince());

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
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Province, object>>[]>()))
                       .ReturnsAsync(GetMockProvince());
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new ProvinceCreateDTO { Name = "Ciudad modificada", CountryId = 1 };

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
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Province, object>>[]>()))
                       .ReturnsAsync((Province?)null);

        var dto = new ProvinceCreateDTO { Name = "Ciudad modificada", CountryId = 1 };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenProvinceExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                       .ReturnsAsync(GetMockProvince());
        _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenProvinceNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Province?)null);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndReturnsTrue_WhenProvinceExists()
    {
        // Arrange
        var Province = GetMockProvince();
        Province.IsActive = false;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(Province);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.True(result);
        Assert.True(Province.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenProvinceNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Province?)null);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.False(result);
    }
}
