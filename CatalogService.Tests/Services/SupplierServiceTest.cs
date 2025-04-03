// SupplierServiceTests.cs - con comentarios AAA (Arrange, Act, Assert)

using CatalogService.Business.Models.Supplier;
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

public class SupplierServiceTests
{
    private readonly Mock<ISupplierRepository> _repositoryMock = new();
    private readonly SupplierService _service;

    public SupplierServiceTests()
    {
        // Arrange: creamos el servicio usando el mock del repositorio
        _service = new SupplierService(_repositoryMock.Object);
    }

    private Supplier GetMockSupplier()
    {
        return new Supplier
        {
            Id = 1,
            TaxId = "XXXX",
            CompanyName = "Test",
            ContactName = "Test",
            Address = "test",
            PostalCodeId = 1,
            PostalCode = new PostalCode 
            { 
                Id = 1, 
                Code = "2000",
                City = new City
                {
                    Id= 1,
                    Name= "Test",
                    Province = new Province
                    {
                        Id  = 1,
                        Name = "Santa Fe",
                        Country = new Country
                        {
                            Id = 1,
                            Name = "Argentina"
                        }
                    }
                }
            },
            PhoneNumber = "test",
            Email = "test",
            IVATypeId = 1,
            IVAType = new IVAType { Id = 1, Description = "test" },
            WarehouseId = 1,
            Warehouse = new Warehouse { Id = 1, Description= "test" },
            TransportId = 1,
            Transport = new Transport { Id = 1, Name = "Test" },
            SellConditionId = 1,
            SellCondition = new SellCondition { Id = 1, Description = "test" },
            IsActive = true
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnMappedDtos()
    {
        // Arrange
        var cities = new List<Supplier> { GetMockSupplier() };
        _repositoryMock.Setup(r => r.GetAllIncludingAsync(It.IsAny<Expression<Func<Supplier, object>>[]>()))
                       .ReturnsAsync(cities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Test", result.First().CompanyName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenSupplierExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Supplier, object>>[]>()))
                       .ReturnsAsync(GetMockSupplier());

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.CompanyName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenSupplierDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Supplier, object>>[]>()))
                       .ReturnsAsync((Supplier?)null);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var dto = new SupplierCreateDTO 
        { 
            CompanyName = "Nueva compania", 
            TaxId = "xxx", 
            ContactName = "algo",
            Address = "asdasd",
            PostalCodeId = 1,
            PhoneNumber = "1234567890",
            Email = "asdas@sdzsds",
            IVATypeId = 1,
            WarehouseId = 1,
            TransportId = 1,
            SellConditionId = 1
        };

        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Supplier, bool>>>()))
                       .ReturnsAsync(new List<Supplier>());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Supplier>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Supplier, object>>[]>()))
                       .ReturnsAsync(GetMockSupplier());

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.CompanyName);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsDto_WhenExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Supplier, object>>[]>()))
                       .ReturnsAsync(GetMockSupplier());
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new SupplierCreateDTO {
            CompanyName = "Compania editada",
            TaxId = "xxx",
            ContactName = "algo",
            Address = "asdasd",
            PostalCodeId = 1,
            PhoneNumber = "1234567890",
            Email = "asdas@sdzsds",
            IVATypeId = 1,
            WarehouseId = 1,
            TransportId = 1,
            SellConditionId = 1
        };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Compania editada", result.CompanyName);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Supplier, object>>[]>()))
                       .ReturnsAsync((Supplier?)null);

        var dto = new SupplierCreateDTO
        {
            CompanyName = "Compania editada",
            TaxId = "xxx",
            ContactName = "algo",
            Address = "asdasd",
            PostalCodeId = 1,
            PhoneNumber = "1234567890",
            Email = "asdas@sdzsds",
            IVATypeId = 1,
            WarehouseId = 1,
            TransportId = 1,
            SellConditionId = 1
        };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenSupplierExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                       .ReturnsAsync(GetMockSupplier());
        _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenSupplierNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Supplier?)null);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndReturnsTrue_WhenSupplierExists()
    {
        // Arrange
        var Supplier = GetMockSupplier();
        Supplier.IsActive = false;
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(Supplier);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.True(result);
        Assert.True(Supplier.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenSupplierNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Supplier?)null);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.False(result);
    }
}
