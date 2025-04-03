// PostalCodeServiceTests.cs - con comentarios AAA (Arrange, Act, Assert)

using CatalogService.Business.Models.PostalCode;
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

public class PostalCodeServiceTests
{
    private readonly Mock<IPostalCodeRepository> _repositoryMock = new();
    private readonly PostalCodeService _service;

    public PostalCodeServiceTests()
    {
        // Arrange: creamos el servicio usando el mock del repositorio
        _service = new PostalCodeService(_repositoryMock.Object);
    }

    private PostalCode GetMockPostalCode()
    {
        return new PostalCode
        {
            Id = 1,
            Code = "2000",
            CityId = 1,
            City = new City
            {
                Name = "Rosario",
                ProvinceId = 1,
                Province = new Province
                {
                    Name = "Santa Fe",
                    CountryId = 1,
                    Country = new Country { Name = "Argentina" }
                }
            },
            IsActive = true
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnMappedDtos()
    {
        // Arrange
        var postalCodes = new List<PostalCode> { GetMockPostalCode() };
        _repositoryMock.Setup(r => r.GetAllIncludingAsync(It.IsAny<Expression<Func<PostalCode, object>>[]>()))
                       .ReturnsAsync(postalCodes);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("2000", result.First().Code);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenPostalCodeExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<PostalCode, object>>[]>()))
                       .ReturnsAsync(GetMockPostalCode());

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("2000", result.Code);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenPostalCodeDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<PostalCode, object>>[]>()))
                       .ReturnsAsync((PostalCode?)null);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var dto = new PostalCodeCreateDTO { Code = "Nuevo  codigo", CityId = 1 };

        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PostalCode, bool>>>()))
                       .ReturnsAsync(new List<PostalCode>());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<PostalCode>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<PostalCode, object>>[]>()))
                       .ReturnsAsync(GetMockPostalCode());

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("2000", result.Code);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsDto_WhenExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<PostalCode, object>>[]>()))
                       .ReturnsAsync(GetMockPostalCode());
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new PostalCodeCreateDTO { Code = "Codigo modificado", CityId = 1 };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Codigo modificado", result.Code);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<PostalCode, object>>[]>()))
                       .ReturnsAsync((PostalCode?)null);

        var dto = new PostalCodeCreateDTO { Code = "Codigo modificado", CityId = 1 };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenPostalCodeExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                       .ReturnsAsync(GetMockPostalCode());
        _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenPostalCodeNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((PostalCode?)null);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndReturnsTrue_WhenPostalCodeExists()
    {
        // Arrange
        var PostalCode = GetMockPostalCode();
        PostalCode.IsActive = false;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(PostalCode);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.True(result);
        Assert.True(PostalCode.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenPostalCodeNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((PostalCode?)null);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.False(result);
    }
}
