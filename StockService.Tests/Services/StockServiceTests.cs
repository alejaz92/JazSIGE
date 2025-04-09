using AuthService.Infrastructure.Models;
using FluentAssertions;
using Moq;
using StockService.Business.Interfaces;
using StockService.Business.Models;
using StockService.Infrastructure.Interfaces;
using StockService.Infrastructure.Models;

namespace StockService.Tests.Services
{
    public class StockServiceTests
    {
        private readonly Mock<IStockRepository> _stockRepositoryMock = new();
        private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock = new();
        private readonly Mock<ICatalogValidatorService> _catalogValidatorServiceMock = new();
        private readonly Mock<IUserServiceClient> _userServiceClientMock = new();
        private readonly StockService.Business.Services.StockService _stockService;

        public StockServiceTests()
        {
            _stockService = new StockService.Business.Services.StockService(
                _stockRepositoryMock.Object,
                _stockMovementRepositoryMock.Object,
                _catalogValidatorServiceMock.Object,
                _userServiceClientMock.Object);
        }

        [Fact]
        public async Task RegisterMovementAsync_Should_Throw_When_Article_Does_Not_Exist()
        {
            // Arrange
            var dto = new StockMovementCreateDTO { ArticleId = 1, MovementType = StockMovementType.Purchase, Quantity = 10 };
            _catalogValidatorServiceMock.Setup(m => m.ArticleExistsAsync(1)).ReturnsAsync(false);

            // Act
            var act = async () => await _stockService.RegisterMovementAsync(dto, 123);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Article with ID 1 does not exist.");
        }

        [Fact]
        public async Task RegisterMovementAsync_Should_Throw_When_FromWarehouse_Does_Not_Exist()
        {
            // Arrange
            var dto = new StockMovementCreateDTO { ArticleId = 1, MovementType = StockMovementType.Purchase, Quantity = 10, FromWarehouseId = 2 };
            _catalogValidatorServiceMock.Setup(m => m.ArticleExistsAsync(1)).ReturnsAsync(true);
            _catalogValidatorServiceMock.Setup(m => m.WarehouseExistsAsync(2)).ReturnsAsync(false);

            // Act
            var act = async () => await _stockService.RegisterMovementAsync(dto, 123);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("From Warehouse with ID 2 does not exist.");
        }

        [Fact]
        public async Task RegisterMovementAsync_Should_Throw_If_ToWarehouse_Is_Missing_For_Purchase()
        {
            // Arrange
            var dto = new StockMovementCreateDTO { ArticleId = 1, MovementType = StockMovementType.Purchase, Quantity = 10 };
            _catalogValidatorServiceMock.Setup(m => m.ArticleExistsAsync(1)).ReturnsAsync(true);

            // Act
            var act = async () => await _stockService.RegisterMovementAsync(dto, 123);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("ToWarehouseId is required for this movement.");
        }

        [Fact]
        public async Task RegisterMovementAsync_Should_Throw_When_Stock_Null_And_Quantity_Negative()
        {
            // Arrange
            var dto = new StockMovementCreateDTO { ArticleId = 1, MovementType = StockMovementType.Sale, Quantity = 10, FromWarehouseId = 3 };
            _catalogValidatorServiceMock.Setup(x => x.ArticleExistsAsync(dto.ArticleId)).ReturnsAsync(true);
            _catalogValidatorServiceMock.Setup(x => x.WarehouseExistsAsync(dto.FromWarehouseId.Value)).ReturnsAsync(true);
            _stockRepositoryMock.Setup(r => r.GetByArticleAndwarehouseAsync(dto.ArticleId, dto.FromWarehouseId.Value)).ReturnsAsync((Stock)null);

            // Act
            var act = async () => await _stockService.RegisterMovementAsync(dto, 1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot create a stock record with negative quantity.");
        }

        [Fact]
        public async Task RegisterMovementAsync_Should_Register_Purchase_Movement()
        {
            // Arrange
            var dto = new StockMovementCreateDTO { ArticleId = 1, Quantity = 10, MovementType = StockMovementType.Purchase, ToWarehouseId = 5 };
            _catalogValidatorServiceMock.Setup(m => m.ArticleExistsAsync(dto.ArticleId)).ReturnsAsync(true);
            _catalogValidatorServiceMock.Setup(m => m.WarehouseExistsAsync(dto.ToWarehouseId.Value)).ReturnsAsync(true);
            _stockRepositoryMock.Setup(r => r.GetByArticleAndwarehouseAsync(dto.ArticleId, dto.ToWarehouseId.Value)).ReturnsAsync((Stock)null);

            // Act
            Func<Task> act = async () => await _stockService.RegisterMovementAsync(dto, 999);

            // Assert
            await act.Should().NotThrowAsync();
            _stockRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Stock>()), Times.Once);
        }

        [Fact]
        public async Task RegisterMovementAsync_Should_Register_Sale_Movement()
        {
            // Arrange
            var dto = new StockMovementCreateDTO { ArticleId = 1, Quantity = 5, MovementType = StockMovementType.Sale, FromWarehouseId = 2 };
            _catalogValidatorServiceMock.Setup(m => m.ArticleExistsAsync(dto.ArticleId)).ReturnsAsync(true);
            _catalogValidatorServiceMock.Setup(m => m.WarehouseExistsAsync(dto.FromWarehouseId.Value)).ReturnsAsync(true);
            _stockRepositoryMock.Setup(r => r.GetByArticleAndwarehouseAsync(dto.ArticleId, dto.FromWarehouseId.Value))
                                .ReturnsAsync(new Stock { Quantity = 10 });

            // Act
            Func<Task> act = async () => await _stockService.RegisterMovementAsync(dto, 123);

            // Assert
            await act.Should().NotThrowAsync();
            _stockRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Stock>(s => s.Quantity == 5)), Times.Once);
        }

        [Fact]
        public async Task RegisterMovementAsync_Should_Register_Transfer_Movement()
        {
            // Arrange
            var dto = new StockMovementCreateDTO
            {
                ArticleId = 1,
                Quantity = 5,
                MovementType = StockMovementType.Transfer,
                FromWarehouseId = 1,
                ToWarehouseId = 2,
                Reference = "REF"
            };

            _catalogValidatorServiceMock.Setup(m => m.ArticleExistsAsync(dto.ArticleId)).ReturnsAsync(true);
            _catalogValidatorServiceMock.Setup(m => m.WarehouseExistsAsync(dto.FromWarehouseId.Value)).ReturnsAsync(true);
            _catalogValidatorServiceMock.Setup(m => m.WarehouseExistsAsync(dto.ToWarehouseId.Value)).ReturnsAsync(true);

            var existingStockFrom = new Stock { ArticleId = dto.ArticleId, WarehouseId = dto.FromWarehouseId.Value, Quantity = 10 };
            _stockRepositoryMock.Setup(r => r.GetByArticleAndwarehouseAsync(dto.ArticleId, dto.FromWarehouseId.Value))
                                .ReturnsAsync(existingStockFrom);
            _stockRepositoryMock.Setup(r => r.GetByArticleAndwarehouseAsync(dto.ArticleId, dto.ToWarehouseId.Value))
                                .ReturnsAsync((Stock)null);

            // Act
            await _stockService.RegisterMovementAsync(dto, 77);

            // Assert
            _stockMovementRepositoryMock.Verify(r => r.AddAsync(It.IsAny<StockMovement>()), Times.Once);
            existingStockFrom.Quantity.Should().Be(5);
            _stockRepositoryMock.Verify(r => r.UpdateAsync(existingStockFrom), Times.Once);
            _stockRepositoryMock.Verify(r => r.AddAsync(It.Is<Stock>(s =>
                s.ArticleId == dto.ArticleId &&
                s.WarehouseId == dto.ToWarehouseId &&
                s.Quantity == dto.Quantity
            )), Times.Once);
        }

        [Fact]
        public async Task RegisterMovementAsync_Should_Register_Adjustment_Movement()
        {
            // Arrange
            var dto = new StockMovementCreateDTO
            {
                ArticleId = 10,
                Quantity = 3,
                MovementType = StockMovementType.Adjustment,
                ToWarehouseId = 7
            };

            _catalogValidatorServiceMock.Setup(m => m.ArticleExistsAsync(dto.ArticleId)).ReturnsAsync(true);
            _catalogValidatorServiceMock.Setup(m => m.WarehouseExistsAsync(dto.ToWarehouseId.Value)).ReturnsAsync(true);
            _stockRepositoryMock.Setup(r => r.GetByArticleAndwarehouseAsync(dto.ArticleId, dto.ToWarehouseId.Value))
                          .ReturnsAsync((Stock)null);

            // Act
            Func<Task> act = async () => await _stockService.RegisterMovementAsync(dto, 42);

            // Assert
            await act.Should().NotThrowAsync();
            _stockRepositoryMock.Verify(r => r.AddAsync(It.Is<Stock>(s =>
                s.ArticleId == dto.ArticleId &&
                s.WarehouseId == dto.ToWarehouseId &&
                s.Quantity == dto.Quantity
            )), Times.Once);
        }
    }
}
