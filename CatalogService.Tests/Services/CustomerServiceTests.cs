// CustomerServiceTests.cs - con comentarios AAA (Arrange, Act, Assert)

using CatalogService.Business.Models.Customer;
using CatalogService.Business.Services;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        // Arrange: configuración básica de mocks e instancias
        _configurationMock.Setup(c => c["GatewayService:BaseUrl"]).Returns("http://fakeapi.com/");
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(new FakeHttpMessageHandler()));

        _service = new CustomerService(
            _repositoryMock.Object,
            _httpClientFactoryMock.Object,
            _configurationMock.Object,
            _httpContextAccessorMock.Object
        );
    }

    private Customer GetMockCustomer()
    {
        // Cliente con todas las relaciones cargadas para evitar nulls
        return new Customer
        {
            Id = 1,
            CompanyName = "Empresa A",
            PostalCode = new PostalCode
            {
                Code = "2000",
                City = new City
                {
                    Name = "Rosario",
                    Province = new Province
                    {
                        Name = "Santa Fe",
                        Country = new Country { Name = "Argentina" }
                    }
                }
            },
            IVAType = new IVAType { Description = "Responsable" },
            Warehouse = new Warehouse { Description = "Central" },
            Transport = new Transport { Name = "Transporte 1" },
            SellCondition = new SellCondition { Description = "Contado" },
            AssignedPriceList = new PriceList { Description = "Minorista" }
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        // Arrange
        var customers = new List<Customer> { GetMockCustomer() };
        _repositoryMock.Setup(r => r.GetAllIncludingAsync(It.IsAny<Expression<Func<Customer, object>>[]>()))
            .ReturnsAsync(customers);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Empresa A", result.First().CompanyName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenCustomerExists()
    {
        // Arrange
        var customer = GetMockCustomer();
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Customer, object>>[]>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Empresa A", result.CompanyName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Customer, object>>[]>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenModelIsValid()
    {
        // Arrange
        var model = new CustomerCreateDTO { CompanyName = "Nueva Empresa", SellerId = 1 };
        var customer = GetMockCustomer();
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Customer>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Customer, object>>[]>())).ReturnsAsync(customer);

        // Act
        var result = await _service.CreateAsync(model);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Empresa A", result.CompanyName);
    }

    [Fact]
    public async Task CreateAsync_ThrowsException_WhenValidationFails()
    {
        // Arrange
        var model = new CustomerCreateDTO { CompanyName = "", SellerId = 999 };

        // Act + Assert
        await Assert.ThrowsAsync<System.Exception>(() => _service.CreateAsync(model));
    }

    [Fact]
    public async Task UpdateAsync_ReturnsDto_WhenCustomerExists()
    {
        // Arrange
        var customer = GetMockCustomer();
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Customer, object>>[]>())).ReturnsAsync(customer);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(1, new CustomerCreateDTO { CompanyName = "Actualizado" });

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Actualizado", result.CompanyName);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<Func<Customer, object>>[]>())).ReturnsAsync((Customer?)null);

        // Act
        var result = await _service.UpdateAsync(99, new CustomerCreateDTO { CompanyName = "Actualizado" });

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenCustomerExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Customer { Id = 1 });
        _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenCustomerNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Customer?)null);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndReturnsTrue_WhenCustomerExists()
    {
        // Arrange
        var customer = new Customer { Id = 1, IsActive = false };
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(customer);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateStatusAsync(1, true);

        // Assert
        Assert.True(result);
        Assert.True(customer.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenCustomerNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Customer?)null);

        // Act
        var result = await _service.UpdateStatusAsync(999, true);

        // Assert
        Assert.False(result);
    }
}

// Cliente HTTP falso para simular llamada al AuthService
public class FakeHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var user = new { FirstName = "Juan", LastName = "Pérez" };
        var json = JsonSerializer.Serialize(user);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }
}
