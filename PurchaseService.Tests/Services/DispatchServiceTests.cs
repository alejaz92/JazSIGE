using FluentAssertions;
using Moq;
using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Models;
using PurchaseService.Business.Services;
using PurchaseService.Infrastructure.Interfaces;
using PurchaseService.Infrastructure.Models;
using Xunit;

namespace PurchaseService.Tests.Services
{
    public class DispatchServiceTests
    {
        private readonly Mock<IDispatchRepository> _dispatchRepositoryMock = new();
        private readonly Mock<ICatalogServiceClient> _catalogServiceClientMock = new();
        private readonly Mock<IUserServiceClient> _userServiceClientMock = new();

        private readonly DispatchService _service;

        public DispatchServiceTests()
        {
            _service = new DispatchService(
                _dispatchRepositoryMock.Object,
                _catalogServiceClientMock.Object,
                _userServiceClientMock.Object
            );
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Dispatches()
        {
            // Arrange
            var dispatches = new List<Dispatch>
            {
                CreateMockDispatch(1)
            };

            _dispatchRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(dispatches);
            SetupCatalogAndUserMocks(dispatches[0]);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_When_None()
        {
            _dispatchRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Dispatch>());

            var result = await _service.GetAllAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_Paginated_Should_Return_Items_And_Total()
        {
            var dispatches = new List<Dispatch>
            {
                CreateMockDispatch(1)
            };

            _dispatchRepositoryMock.Setup(r => r.GetAllAsync(1, 10)).ReturnsAsync(dispatches);
            _dispatchRepositoryMock.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(1);
            SetupCatalogAndUserMocks(dispatches[0]);

            var (items, total) = await _service.GetAllAsync(1, 10);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Paginated_Should_Return_Empty_When_None()
        {
            _dispatchRepositoryMock.Setup(r => r.GetAllAsync(1, 10)).ReturnsAsync(new List<Dispatch>());
            _dispatchRepositoryMock.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(0);

            var (items, total) = await _service.GetAllAsync(1, 10);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Dispatch_When_Found()
        {
            var dispatch = CreateMockDispatch(5);
            _dispatchRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(dispatch);
            SetupCatalogAndUserMocks(dispatch);

            var result = await _service.GetByIdAsync(5);

            result.Should().NotBeNull();
            result!.Id.Should().Be(5);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
        {
            _dispatchRepositoryMock.Setup(r => r.GetByIdAsync(123)).ReturnsAsync((Dispatch?)null);

            var result = await _service.GetByIdAsync(123);

            result.Should().BeNull();
        }

        // --- Helpers internos ---

        private Dispatch CreateMockDispatch(int id)
        {
            return new Dispatch
            {
                Id = id,
                Code = $"D{id:000}",
                Origin = "China",
                Date = DateTime.Today,
                PurchaseId = id,
                Purchase = new Purchase
                {
                    Id = id,
                    Date = DateTime.Today,
                    SupplierId = 10,
                    WarehouseId = 20,
                    UserId = 30,
                    Articles = new List<Purchase_Article>
                    {
                        new Purchase_Article { ArticleId = 100, Quantity = 5, UnitCost = 10 }
                    }
                }
            };
        }

        private void SetupCatalogAndUserMocks(Dispatch dispatch)
        {
            _catalogServiceClientMock.Setup(c => c.GetSupplierNameAsync(dispatch.Purchase.SupplierId)).ReturnsAsync("Proveedor");
            _catalogServiceClientMock.Setup(c => c.GetWarehouseNameAsync(dispatch.Purchase.WarehouseId)).ReturnsAsync("Depósito");
            _catalogServiceClientMock.Setup(c => c.GetArticleNameAsync(It.IsAny<int>())).ReturnsAsync("Artículo");
            _userServiceClientMock.Setup(u => u.GetUserNameAsync(dispatch.Purchase.UserId)).ReturnsAsync("Usuario");
        }
    }
}
