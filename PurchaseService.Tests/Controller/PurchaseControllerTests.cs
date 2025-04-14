using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PurchaseService.Business.Exceptions;
using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Models;
using PurchaseService.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseService.Tests.Controller
{
    public class PurchaseControllerTests
    {
        private readonly Mock<IPurchaseService> _purchaseServiceMock = new();
        private readonly PurchaseController _controller;

        public PurchaseControllerTests()
        {
            _controller = new PurchaseController(_purchaseServiceMock.Object);
        }


        // create tests
        [Fact]
        public async Task Create_Should_Return_Created_When_Success()
        {
            // Arrange
            var dto = new PurchaseCreateDTO();
            var userId = 123;
            _purchaseServiceMock.Setup(s => s.CreateAsync(dto, userId)).ReturnsAsync(1);
            SetUserIdHeader(userId);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.Value.Should().BeEquivalentTo(new { id = 1 });
        }

        [Fact]
        public async Task Create_Should_Return_Unauthorized_When_No_UserId_Header()
        {
            // Arrange
            var dto = new PurchaseCreateDTO();
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorized.Value.Should().BeEquivalentTo(new { error = "Usuario no autenticado correctamente" });
        }

        [Fact]
        public async Task Create_Should_Return_BadRequest_When_ArgumentException()
        {
            // Arrange
            var dto = new PurchaseCreateDTO();
            var userId = 123;
            _purchaseServiceMock.Setup(s => s.CreateAsync(dto, userId)).ThrowsAsync(new ArgumentException("Invalid input"));
            SetUserIdHeader(userId);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().BeEquivalentTo(new { error = "Invalid input" });
        }

        [Fact]
        public async Task Create_Should_Return_500_When_PartialSuccessException()
        {
            // Arrange
            var dto = new PurchaseCreateDTO();
            var userId = 123;
            _purchaseServiceMock.Setup(s => s.CreateAsync(dto, userId)).ThrowsAsync(new PartialSuccessException("Stock falló"));
            SetUserIdHeader(userId);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var serverError = result.Should().BeOfType<ObjectResult>().Subject;
            serverError.StatusCode.Should().Be(500);
            serverError.Value.Should().BeEquivalentTo(new { error = "Stock falló" });
        }

        [Fact]
        public async Task Create_Should_Return_500_When_Unexpected_Exception()
        {
            // Arrange
            var dto = new PurchaseCreateDTO();
            var userId = 123;
            _purchaseServiceMock.Setup(s => s.CreateAsync(dto, userId)).ThrowsAsync(new Exception("Error inesperado"));
            SetUserIdHeader(userId);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var serverError = result.Should().BeOfType<ObjectResult>().Subject;
            serverError.StatusCode.Should().Be(500);
            serverError.Value.Should().BeEquivalentTo(new { error = "Unexpected error", detail = "Error inesperado" });
        }

        // retry-stock tests
        [Fact]
        public async Task RetryStockUpdate_Should_Return_Ok_When_Success_And_Admin()
        {
            // Arrange
            int purchaseId = 1;
            int userId = 123;
            SetHttpContextWithUser(userId, isAdmin: true);

            _purchaseServiceMock.Setup(s => s.RetryStockUpdateAsync(purchaseId, userId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.RetryStockUpdate(purchaseId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { message = "Stock actualizado correctamente." });
        }

        [Fact]
        public async Task RetryStockUpdate_Should_Return_401_When_UserId_Header_Missing()
        {
            // Arrange
            SetHttpContextWithUser(userId: null, isAdmin: true); // Header faltante

            // Act
            var result = await _controller.RetryStockUpdate(1);

            // Assert
            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorized.Value.Should().BeEquivalentTo(new { error = "Usuario no autenticado correctamente" });
        }

        [Fact]
        public async Task RetryStockUpdate_Should_Return_403_When_Not_Admin()
        {
            // Arrange
            SetHttpContextWithUser(userId: 1, isAdmin: false);

            // Act
            var result = await _controller.RetryStockUpdate(1);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task RetryStockUpdate_Should_Return_404_When_ArgumentException()
        {
            // Arrange
            int purchaseId = 99;
            int userId = 123;
            SetHttpContextWithUser(userId, isAdmin: true);

            _purchaseServiceMock.Setup(s => s.RetryStockUpdateAsync(purchaseId, userId))
                                .ThrowsAsync(new ArgumentException("No existe"));

            // Act
            var result = await _controller.RetryStockUpdate(purchaseId);

            // Assert
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFound.Value.Should().BeEquivalentTo(new { error = "No existe" });
        }

        [Fact]
        public async Task RetryStockUpdate_Should_Return_400_When_InvalidOperationException()
        {
            int userId = 123;
            SetHttpContextWithUser(userId, isAdmin: true);

            _purchaseServiceMock.Setup(s => s.RetryStockUpdateAsync(5, userId))
                                .ThrowsAsync(new InvalidOperationException("Ya fue actualizado"));

            var result = await _controller.RetryStockUpdate(5);

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().BeEquivalentTo(new { error = "Ya fue actualizado" });
        }

        [Fact]
        public async Task RetryStockUpdate_Should_Return_500_When_Unexpected_Exception()
        {
            int userId = 123;
            SetHttpContextWithUser(userId, isAdmin: true);

            _purchaseServiceMock.Setup(s => s.RetryStockUpdateAsync(7, userId))
                                .ThrowsAsync(new Exception("Boom"));

            var result = await _controller.RetryStockUpdate(7);

            var error = result.Should().BeOfType<ObjectResult>().Subject;
            error.StatusCode.Should().Be(500);
            error.Value.Should().BeEquivalentTo(new { error = "Unexpected error", detail = "Boom" });
        }

        // retry-all-pending-stock tests

        [Fact]
        public async Task RetryAllPendingStock_Should_Return_Ok_When_Success()
        {
            // Arrange
            int userId = 123;
            SetHttpContextWithUser(userId, isAdmin: true);

            _purchaseServiceMock.Setup(s => s.RetryAllPendingStockAsync(userId)).ReturnsAsync(3);

            // Act
            var result = await _controller.RetryAllPendingStock();

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(new { message = "Se actualizaron correctamente 3 compras." });
        }

        [Fact]
        public async Task RetryAllPendingStock_Should_Return_401_When_UserId_Header_Missing()
        {
            // Arrange
            SetHttpContextWithUser(userId: null, isAdmin: true);

            // Act
            var result = await _controller.RetryAllPendingStock();

            // Assert
            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorized.Value.Should().BeEquivalentTo(new { error = "Usuario no autenticado correctamente" });
        }

        [Fact]
        public async Task RetryAllPendingStock_Should_Return_403_When_Not_Admin()
        {
            // Arrange
            SetHttpContextWithUser(userId: 123, isAdmin: false);

            // Act
            var result = await _controller.RetryAllPendingStock();

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task RetryAllPendingStock_Should_Return_500_When_Exception_Thrown()
        {
            // Arrange
            int userId = 123;
            SetHttpContextWithUser(userId, isAdmin: true);

            _purchaseServiceMock.Setup(s => s.RetryAllPendingStockAsync(userId))
                                .ThrowsAsync(new Exception("Boom"));

            // Act
            var result = await _controller.RetryAllPendingStock();

            // Assert
            var error = result.Should().BeOfType<ObjectResult>().Subject;
            error.StatusCode.Should().Be(500);
            error.Value.Should().BeEquivalentTo(new { error = "Unexpected error", detail = "Boom" });
        }

        // pending-stock tests
        [Fact]
        public async Task GetPendingStock_Should_Return_Ok_When_Admin()
        {
            // Arrange
            SetHttpContextWithUser(userId: 1, isAdmin: true);

            var list = new List<PurchaseDTO> { new PurchaseDTO { Id = 1 } };
            _purchaseServiceMock.Setup(s => s.GetPendingStockAsync()).ReturnsAsync(list);

            // Act
            var result = await _controller.GetPendingStock();

            // Assert
            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(list);
        }

        [Fact]
        public async Task GetPendingStock_Should_Return_403_When_Not_Admin()
        {
            // Arrange
            SetHttpContextWithUser(userId: 1, isAdmin: false);

            // Act
            var result = await _controller.GetPendingStock();

            // Assert
            result.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetPendingStock_Should_Return_500_When_Exception()
        {
            // Arrange
            SetHttpContextWithUser(userId: 1, isAdmin: true);

            _purchaseServiceMock.Setup(s => s.GetPendingStockAsync()).ThrowsAsync(new Exception("Fallo interno"));

            // Act
            var result = await _controller.GetPendingStock();

            // Assert
            var error = result.Result.Should().BeOfType<ObjectResult>().Subject;
            error.StatusCode.Should().Be(500);
            error.Value.Should().BeEquivalentTo(new { error = "Unexpected error", detail = "Fallo interno" });
        }




        // --- Helpers
        private void SetUserIdHeader(int userId)
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.ControllerContext.HttpContext.Request.Headers["X-UserId"] = userId.ToString();
        }

        private void SetHttpContextWithUser(int? userId, bool isAdmin)
        {
            var context = new DefaultHttpContext();

            if (userId.HasValue)
                context.Request.Headers["X-UserId"] = userId.Value.ToString();

            var claims = new List<Claim>();
            if (isAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            var identity = new ClaimsIdentity(claims, "TestAuth");
            context.User = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
        }

    }
}
