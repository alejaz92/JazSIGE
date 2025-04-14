using FluentAssertions;
using Moq;
using PurchaseService.Business.Exceptions;
using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Models;
using PurchaseService.Business.Services;
using PurchaseService.Infrastructure.Interfaces;
using PurchaseService.Infrastructure.Models;
using System.Collections.Generic;
using Xunit;

namespace PurchaseService.Tests.Services
{
    public class PurchaseServiceTests
    {
        private readonly Mock<IPurchaseRepository> _purchaseRepositoryMock = new();
        private readonly Mock<IDispatchRepository> _dispatchRepositoryMock = new();
        private readonly Mock<ICatalogServiceClient> _catalogServiceMock = new();
        private readonly Mock<IUserServiceClient> _userServiceMock = new();
        private readonly Mock<IStockServiceClient> _stockServiceClientMock = new();

        private readonly PurchaseService.Business.Services.PurchaseService _service;

        public PurchaseServiceTests()
        {
            _service = new PurchaseService.Business.Services.PurchaseService(
                _purchaseRepositoryMock.Object,
                _dispatchRepositoryMock.Object,
                _catalogServiceMock.Object,
                _userServiceMock.Object,
                _stockServiceClientMock.Object
            );
        }


        //CreateAsync Tests
        [Fact]
        public async Task CreateAsync_Should_Create_Purchase_Without_Dispatch()
        {
            var dto = GetValidDto();
            int userId = 1;

            SetupValidCatalogAndUser(dto, userId);

            _purchaseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Purchase>()))
                                   .Returns<Purchase>(p =>
                                   {
                                       p.Id = 100;
                                       return Task.FromResult(p);
                                   });

            _purchaseRepositoryMock.Setup(r => r.SaveChangesAsync())
                                   .Returns(Task.CompletedTask);

            _stockServiceClientMock.Setup(s => s.RegisterPurchaseMovementsAsync(userId, dto.WarehouseId, It.IsAny<List<(int articleId, decimal quantity)>>()))

                                   .Returns(Task.CompletedTask);

            var result = await _service.CreateAsync(dto, userId);

            result.Should().Be(100);
            _dispatchRepositoryMock.Verify(d => d.AddAsync(It.IsAny<Dispatch>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_Should_Create_Purchase_With_Dispatch()
        {
            var dto = GetValidDto();
            dto.Dispatch = new DispatchCreateDTO { Code = "D001", Date = DateTime.Today, Origin = "Import" };
            int userId = 1;

            SetupValidCatalogAndUser(dto, userId);

            _purchaseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Purchase>()))
                                   .Returns<Purchase>(p =>
                                   {
                                       p.Id = 101;
                                       return Task.FromResult(p);
                                   });

            _purchaseRepositoryMock.Setup(r => r.SaveChangesAsync())
                                   .Returns(Task.CompletedTask);

            _stockServiceClientMock.Setup(s => s.RegisterPurchaseMovementsAsync(userId, dto.WarehouseId, It.IsAny<List<(int articleId, decimal quantity)>>())).Returns(Task.CompletedTask);

            var result = await _service.CreateAsync(dto, userId);

            result.Should().Be(101);
            _dispatchRepositoryMock.Verify(d => d.AddAsync(It.IsAny<Dispatch>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_If_Supplier_Invalid()
        {
            var dto = GetValidDto();
            int userId = 1;

            _catalogServiceMock.Setup(c => c.GetSupplierNameAsync(dto.SupplierId)).ReturnsAsync((string?)null);

            var act = async () => await _service.CreateAsync(dto, userId);

            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("*Supplier*");
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_If_Warehouse_Invalid()
        {
            var dto = GetValidDto();
            int userId = 1;

            _catalogServiceMock.Setup(c => c.GetSupplierNameAsync(dto.SupplierId)).ReturnsAsync("OK");
            _catalogServiceMock.Setup(c => c.GetWarehouseNameAsync(dto.WarehouseId)).ReturnsAsync((string?)null);

            var act = async () => await _service.CreateAsync(dto, userId);

            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("*Warehouse*");
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_If_User_Invalid()
        {
            var dto = GetValidDto();
            int userId = 1;

            SetupValidCatalogAndUser(dto, userId);
            _userServiceMock.Setup(x => x.GetUserNameAsync(userId)).ReturnsAsync((string?)null);

            var act = async () => await _service.CreateAsync(dto, userId);

            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("*User*");
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_If_Article_Invalid()
        {
            var dto = GetValidDto();
            int userId = 1;

            SetupValidCatalogAndUser(dto, userId);
            _catalogServiceMock.Setup(x => x.GetArticleNameAsync(It.IsAny<int>())).ReturnsAsync((string?)null);

            var act = async () => await _service.CreateAsync(dto, userId);

            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("*Article*");
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_PartialSuccess_If_Stock_Fails()
        {
            var dto = GetValidDto();
            int userId = 1;

            SetupValidCatalogAndUser(dto, userId);

            _purchaseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Purchase>()))
                                   .Returns<Purchase>(p =>
                                   {
                                       p.Id = 777;
                                       return Task.FromResult(p);
                                   });

            _purchaseRepositoryMock.Setup(r => r.SaveChangesAsync())
                                   .Returns(Task.CompletedTask);

            _stockServiceClientMock.Setup(s => s.RegisterPurchaseMovementsAsync(userId, dto.WarehouseId, It.IsAny<List<(int articleId, decimal quantity)>>()))

                                   .ThrowsAsync(new Exception("Fallo stock"));

            var act = async () => await _service.CreateAsync(dto, userId);

            await act.Should().ThrowAsync<PartialSuccessException>()
                     .WithMessage("*stock*");
        }

        //RetryStockUpdateAsync tests

        [Fact]
        public async Task RetryStockUpdateAsync_Should_UpdateStock_When_Valid()
        {
            // Arrange
            int userId = 1;
            int purchaseId = 101;

            var purchase = new Purchase
            {
                Id = purchaseId,
                SupplierId = 1,
                WarehouseId = 2,
                UserId = userId,
                StockUpdated = false,
                Articles = new List<Purchase_Article>
        {
            new Purchase_Article { ArticleId = 10, Quantity = 5, UnitCost = 100 }
        }
            };

            _purchaseRepositoryMock.Setup(r => r.GetByIdAsync(purchaseId)).ReturnsAsync(purchase);
            _catalogServiceMock.Setup(c => c.GetArticleNameAsync(10)).ReturnsAsync("Artículo");
            _stockServiceClientMock.Setup(s => s.RegisterPurchaseMovementsAsync(
                userId,
                purchase.WarehouseId,
                It.Is<List<(int articleId, decimal quantity)>>(l => l.Any(x => x.articleId == 10 && x.quantity == 5))))
                .Returns(Task.CompletedTask);

            _purchaseRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _service.RetryStockUpdateAsync(purchaseId, userId);

            // Assert
            await act.Should().NotThrowAsync();
            purchase.StockUpdated.Should().BeTrue();
        }

        [Fact]
        public async Task RetryStockUpdateAsync_Should_Throw_If_Purchase_Not_Exist()
        {
            int purchaseId = 999;
            int userId = 1;

            _purchaseRepositoryMock.Setup(r => r.GetByIdAsync(purchaseId)).ReturnsAsync((Purchase?)null);

            var act = async () => await _service.RetryStockUpdateAsync(purchaseId, userId);

            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("*Purchase with ID*");
        }

        [Fact]
        public async Task RetryStockUpdateAsync_Should_Throw_If_Stock_Already_Updated()
        {
            int purchaseId = 102;
            int userId = 1;

            var purchase = new Purchase
            {
                Id = purchaseId,
                StockUpdated = true
            };

            _purchaseRepositoryMock.Setup(r => r.GetByIdAsync(purchaseId)).ReturnsAsync(purchase);

            var act = async () => await _service.RetryStockUpdateAsync(purchaseId, userId);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("*has already been updated*");
        }

        [Fact]
        public async Task RetryStockUpdateAsync_Should_Throw_If_Article_No_Longer_Exists()
        {
            int purchaseId = 103;
            int userId = 1;

            var purchase = new Purchase
            {
                Id = purchaseId,
                StockUpdated = false,
                Articles = new List<Purchase_Article>
        {
            new Purchase_Article { ArticleId = 10, Quantity = 5 }
        }
            };

            _purchaseRepositoryMock.Setup(r => r.GetByIdAsync(purchaseId)).ReturnsAsync(purchase);
            _catalogServiceMock.Setup(c => c.GetArticleNameAsync(10)).ReturnsAsync((string?)null);

            var act = async () => await _service.RetryStockUpdateAsync(purchaseId, userId);

            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("*no longer exists*");
        }

        // RetryAllPendingStockAsync tests
        [Fact]
        public async Task RetryAllPendingStockAsync_Should_Update_All_When_Valid()
        {
            // Arrange
            int userId = 1;
            var purchases = new List<Purchase>
            {
                new Purchase
                {
                    Id = 1,
                    WarehouseId = 1,
                    Articles = new List<Purchase_Article>
                    {
                        new Purchase_Article { ArticleId = 10, Quantity = 5 }
                    }
                },
                new Purchase
                {
                    Id = 2,
                    WarehouseId = 1,
                    Articles = new List<Purchase_Article>
                    {
                        new Purchase_Article { ArticleId = 20, Quantity = 3 }
                    }
                }
            };

            _purchaseRepositoryMock.Setup(r => r.GetPendingStockAsync()).ReturnsAsync(purchases);
            _stockServiceClientMock.Setup(s => s.RegisterPurchaseMovementsAsync(
                userId, It.IsAny<int>(), It.IsAny<List<(int, decimal)>>()))
                .Returns(Task.CompletedTask);
            _purchaseRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.RetryAllPendingStockAsync(userId);

            // Assert
            result.Should().Be(2);
            purchases.All(p => p.StockUpdated).Should().BeTrue();
        }


        [Fact]
        public async Task RetryAllPendingStockAsync_Should_Skip_Failed_And_Continue()
        {
            // Arrange
            int userId = 1;
            var purchases = new List<Purchase>
            {
                new Purchase
                {
                    Id = 1,
                    WarehouseId = 1,
                    Articles = new List<Purchase_Article>
                    {
                        new Purchase_Article { ArticleId = 10, Quantity = 5 }
                    }
                },
                new Purchase
                {
                    Id = 2,
                    WarehouseId = 1,
                    Articles = new List<Purchase_Article>
                    {
                        new Purchase_Article { ArticleId = 20, Quantity = 3 }
                    }
                }
            };

            _purchaseRepositoryMock.Setup(r => r.GetPendingStockAsync()).ReturnsAsync(purchases);
            _stockServiceClientMock.Setup(s => s.RegisterPurchaseMovementsAsync(userId, 1,
                It.Is<List<(int, decimal)>>(l => l.Any(x => x.Item1 == 10))))
                .Returns(Task.CompletedTask);
            _stockServiceClientMock.Setup(s => s.RegisterPurchaseMovementsAsync(userId, 1,
                It.Is<List<(int, decimal)>>(l => l.Any(x => x.Item1 == 20))))
                .ThrowsAsync(new Exception("error"));

            _purchaseRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.RetryAllPendingStockAsync(userId);

            // Assert
            result.Should().Be(1);
            purchases[0].StockUpdated.Should().BeTrue();
            purchases[1].StockUpdated.Should().BeFalse();
        }

        [Fact]
        public async Task RetryAllPendingStockAsync_Should_Return_Zero_When_All_Fail()
        {
            int userId = 1;
            var purchases = new List<Purchase>
            {
                new Purchase
                {
                    Id = 1,
                    WarehouseId = 1,
                    Articles = new List<Purchase_Article>
                    {
                        new Purchase_Article { ArticleId = 10, Quantity = 5 }
                    }
                }
            };

            _purchaseRepositoryMock.Setup(r => r.GetPendingStockAsync()).ReturnsAsync(purchases);
            _stockServiceClientMock.Setup(s => s.RegisterPurchaseMovementsAsync(userId, 1, It.IsAny<List<(int articleId, decimal quantity)>>())).ThrowsAsync(new Exception("fail"));
            _purchaseRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.RetryAllPendingStockAsync(userId);

            result.Should().Be(0);
            purchases[0].StockUpdated.Should().BeFalse();
        }

        // getAllAsyn no pagination tests
        [Fact]
        public async Task GetAllAsync_Should_Return_Purchases()
        {
            var purchases = new List<Purchase>
            {
                new Purchase
                {
                    Id = 1,
                    SupplierId = 1,
                    WarehouseId = 2,
                    UserId = 3,
                    Articles = new List<Purchase_Article>()
                }
            };

            _purchaseRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(purchases);
            _catalogServiceMock.Setup(c => c.GetSupplierNameAsync(It.IsAny<int>())).ReturnsAsync("Supplier");
            _catalogServiceMock.Setup(c => c.GetWarehouseNameAsync(It.IsAny<int>())).ReturnsAsync("Warehouse");
            _catalogServiceMock.Setup(c => c.GetArticleNameAsync(It.IsAny<int>())).ReturnsAsync("Artículo");
            _userServiceMock.Setup(u => u.GetUserNameAsync(It.IsAny<int>())).ReturnsAsync("Usuario");

            var result = await _service.GetAllAsync();

            result.Should().HaveCount(1);
        }

        // GetAllAsync with pagination tests
        [Fact]
        public async Task GetAllAsync_Paginated_Should_Return_Purchases_And_TotalCount()
        {
            var purchases = new List<Purchase>
            {
                new Purchase
                {
                    Id = 1,
                    SupplierId = 1,
                    WarehouseId = 2,
                    UserId = 3,
                    Articles = new List<Purchase_Article>()
                }
            };

            _purchaseRepositoryMock.Setup(r => r.GetAllAsync(1, 10)).ReturnsAsync(purchases);
            _purchaseRepositoryMock.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(1);
            _catalogServiceMock.Setup(c => c.GetSupplierNameAsync(It.IsAny<int>())).ReturnsAsync("Supplier");
            _catalogServiceMock.Setup(c => c.GetWarehouseNameAsync(It.IsAny<int>())).ReturnsAsync("Warehouse");
            _catalogServiceMock.Setup(c => c.GetArticleNameAsync(It.IsAny<int>())).ReturnsAsync("Artículo");
            _userServiceMock.Setup(u => u.GetUserNameAsync(It.IsAny<int>())).ReturnsAsync("Usuario");

            var (result, totalCount) = await _service.GetAllAsync(1, 10);

            result.Should().HaveCount(1);
            totalCount.Should().Be(1);
        }

        // GetByIdAsync tests
        [Fact]
        public async Task GetByIdAsync_Should_Return_Purchase_When_Found()
        {
            var purchase = new Purchase
            {
                Id = 1,
                SupplierId = 1,
                WarehouseId = 2,
                UserId = 3,
                Articles = new List<Purchase_Article>()
            };

            _purchaseRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(purchase);
            _catalogServiceMock.Setup(c => c.GetSupplierNameAsync(It.IsAny<int>())).ReturnsAsync("Supplier");
            _catalogServiceMock.Setup(c => c.GetWarehouseNameAsync(It.IsAny<int>())).ReturnsAsync("Warehouse");
            _catalogServiceMock.Setup(c => c.GetArticleNameAsync(It.IsAny<int>())).ReturnsAsync("Artículo");
            _userServiceMock.Setup(u => u.GetUserNameAsync(It.IsAny<int>())).ReturnsAsync("Usuario");

            var result = await _service.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
        {
            _purchaseRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Purchase?)null);

            var result = await _service.GetByIdAsync(1);

            result.Should().BeNull();
        }

        // GetPendingStockAsync tests
        [Fact]
        public async Task GetPendingStockAsync_Should_Return_Purchases_With_StockUpdated_False()
        {
            var purchases = new List<Purchase>
            {
                new Purchase
                {
                    Id = 1,
                    StockUpdated = false,
                    SupplierId = 1,
                    WarehouseId = 2,
                    UserId = 3,
                    Articles = new List<Purchase_Article>()
                }
            };

            _purchaseRepositoryMock.Setup(r => r.GetPendingStockAsync()).ReturnsAsync(purchases);
            _catalogServiceMock.Setup(c => c.GetSupplierNameAsync(It.IsAny<int>())).ReturnsAsync("Supplier");
            _catalogServiceMock.Setup(c => c.GetWarehouseNameAsync(It.IsAny<int>())).ReturnsAsync("Warehouse");
            _catalogServiceMock.Setup(c => c.GetArticleNameAsync(It.IsAny<int>())).ReturnsAsync("Artículo");
            _userServiceMock.Setup(u => u.GetUserNameAsync(It.IsAny<int>())).ReturnsAsync("Usuario");

            var result = await _service.GetPendingStockAsync();

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetPendingStockAsync_Should_Return_Empty_When_None()
        {
            _purchaseRepositoryMock.Setup(r => r.GetPendingStockAsync()).ReturnsAsync(new List<Purchase>());

            var result = await _service.GetPendingStockAsync();

            result.Should().BeEmpty();
        }




        // --- Helpers ---

        private PurchaseCreateDTO GetValidDto() => new()
        {
            Date = DateTime.Today,
            SupplierId = 1,
            WarehouseId = 2,
            Dispatch = null,
            Articles = new List<PurchaseArticleCreateDTO>
            {
                new PurchaseArticleCreateDTO { ArticleId = 10, Quantity = 5, UnitCost = 100 }
            }
        };

        private void SetupValidCatalogAndUser(PurchaseCreateDTO dto, int userId)
        {
            _catalogServiceMock.Setup(c => c.GetSupplierNameAsync(dto.SupplierId)).ReturnsAsync("OK");
            _catalogServiceMock.Setup(c => c.GetWarehouseNameAsync(dto.WarehouseId)).ReturnsAsync("OK");
            _catalogServiceMock.Setup(a => a.GetArticleNameAsync(It.IsAny<int>())).ReturnsAsync("Artículo");
            _userServiceMock.Setup(u => u.GetUserNameAsync(userId)).ReturnsAsync("Usuario");
        }
    }
}
