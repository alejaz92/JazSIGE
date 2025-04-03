// TransportServiceTests.cs - con comentarios AAA (Arrange, Act, Assert)

using CatalogService.Business.Models.Transport;
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

public class TransportServiceTests
{
    private readonly Mock<ITransportRepository> _repositoryMock = new();
    private readonly TransportService _service;

    public TransportServiceTests()
    {
        // Arrange: creamos el servicio usando el mock del repositorio
        _service = new TransportService(_repositoryMock.Object);
    }

    private Transport GetMockTransport()
    {
        return new Transport
        {
            Id = 1,
            Name = "Transporte 1",
            TaxId = "xxxxx",
            Address = "algo 123",
            PostalCode = new PostalCode { Id = 2, Code = "2000" },
            PhoneNumber = "123123123",
            Email = "asdasd.asdasd",
            Comment = "asldsada",
            IsActive = true
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnMappedDtos()
    {
        // Arrange
        var cities = new List<Transport> { GetMockTransport() };
        _repositoryMock.Setup(r => r.GetAllIncludingAsync(It.IsAny<Expression<Func<Transport, object>>[]>()))
                       .ReturnsAsync(cities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Transporte 1", result.First().Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenTransportExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Transport, object>>[]>()))
                       .ReturnsAsync(GetMockTransport());

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Transporte 1", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenTransportDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Transport, object>>[]>()))
                       .ReturnsAsync((Transport?)null);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var dto = new TransportCreateDTO
        {
            Name = "Transporte 2",
            TaxId = "xxxxx",
            Address = "algo 123",
            PostalCodeId = 1,
            PhoneNumber = "123123123",
            Email = "asdasd.asdasd",
            Comment = "asldsada",
        };

        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Transport, bool>>>()))
                       .ReturnsAsync(new List<Transport>());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Transport>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Transport, object>>[]>()))
                       .ReturnsAsync(GetMockTransport());

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Transporte 1", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsDto_WhenExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Transport, object>>[]>()))
                       .ReturnsAsync(GetMockTransport());
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new TransportCreateDTO {
            Name = "Transporte 2",
            TaxId = "xxxxx",
            Address = "algo 123",
            PostalCodeId = 1,
            PhoneNumber = "123123123",
            Email = "asdasd.asdasd",
            Comment = "asldsada",
        };


        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Transporte 2", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Transport, object>>[]>()))
                       .ReturnsAsync((Transport?)null);

        var dto = new TransportCreateDTO {
            Name = "Transporte 2",
            TaxId = "xxxxx",
            Address = "algo 123",
            PostalCodeId = 1,
            PhoneNumber = "123123123",
            Email = "asdasd.asdasd",
            Comment = "asldsada",
        };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenTransportExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                       .ReturnsAsync(GetMockTransport());
        _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenTransportNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Transport?)null);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndReturnsTrue_WhenTransportExists()
    {
        // Arrange
        var Transport = GetMockTransport();
        Transport.IsActive = false;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(Transport);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.True(result);
        Assert.True(Transport.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenTransportNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Transport?)null);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.False(result);
    }
}
