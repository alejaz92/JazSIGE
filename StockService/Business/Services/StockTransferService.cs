using Microsoft.EntityFrameworkCore;
using StockService.Business.Interfaces;
using StockService.Business.Models;
using StockService.Business.Models.StockTransfer;
using StockService.Infrastructure.Interfaces;
using StockService.Infrastructure.Models;

namespace StockService.Business.Services
{
    public class StockTransferService : IStockTransferService
    {
        private readonly IStockTransferRepository _stockTransferRepository;
        private readonly IStockService _stockService;
        private readonly ICatalogValidatorService _catalogValidatorService;

        public StockTransferService(
            IStockTransferRepository stockTransferRepository,
            IStockService stockService,
            ICatalogValidatorService catalogValidatorService)
        {
            _stockTransferRepository = stockTransferRepository;
            _stockService = stockService;
            _catalogValidatorService = catalogValidatorService;
        }

        public async Task<int> CreateAsync(StockTransferCreateDTO dto, int userId)
        {

            // get code
            var code = await _stockTransferRepository.GenerateNextCodeAsync();

            // 1. Armar la transferencia
            var transfer = new StockTransfer
            {
                Code = code,
                Date = dto.Date,
                OriginWarehouseId = dto.OriginWarehouseId,
                DestinationWarehouseId = dto.DestinationWarehouseId,
                TransportId = dto.TransportId,
                NumberOfPackages = dto.NumberOfPackages,
                DeclaredValue = dto.DeclaredValue,
                Observations = dto.Observations,
                UserId = userId,
                Articles = dto.Articles.Select(a => new StockTransfer_Article
                {
                    ArticleId = a.ArticleId,
                    Quantity = a.Quantity
                }).ToList()
            };

            // 2. Guardar primero la transferencia
            await _stockTransferRepository.AddAsync(transfer);

            // 3. Registrar los movimientos vía StockService
            foreach (var item in dto.Articles)
            {
                var movementOut = new StockMovementCreateDTO
                {
                    ArticleId = item.ArticleId,
                    Quantity = item.Quantity,
                    MovementType = StockMovementType.Transfer,
                    FromWarehouseId = dto.OriginWarehouseId,
                    ToWarehouseId = dto.DestinationWarehouseId,
                    Reference = $"Transferencia {code}",
                    StockTransferId = transfer.Id
                };


                await _stockService.RegisterMovementAsync(movementOut, userId);

            }

            return transfer.Id;
        }
        public async Task<StockTransferDTO?> GetByIdAsync(int id)
        {
            var transfer = await _stockTransferRepository.GetByIdAsync(id);
            if (transfer == null) return null;

            // Obtener nombres de artículos uno por uno (llamadas individuales)
            var articles = new List<StockTransferArticleDTO>();

            var originWarehouseName = await _catalogValidatorService.GetWarehouseNameAsync(transfer.OriginWarehouseId);
            var destinationWarehouseName = await _catalogValidatorService.GetWarehouseNameAsync(transfer.DestinationWarehouseId);

            string transportName = "";

            if (transfer.TransportId != null)
            {
                transportName = await _catalogValidatorService.GetTransportNameAsync(transfer.TransportId.Value);
            }


            foreach (var a in transfer.Articles)
            {
                var name = await _catalogValidatorService.GetArticleNameAsync(a.ArticleId);
                articles.Add(new StockTransferArticleDTO
                {
                    ArticleId = a.ArticleId,
                    ArticleName = name ?? string.Empty,
                    Quantity = a.Quantity
                });
            }

            return new StockTransferDTO
            {
                Id = transfer.Id,
                Code = transfer.Code,
                Date = transfer.Date,
                OriginWarehouseId = transfer.OriginWarehouseId,
                OriginWarehouseName = originWarehouseName,
                DestinationWarehouseId = transfer.DestinationWarehouseId,
                DestinationWarehouseName = destinationWarehouseName,
                TransportId = transfer.TransportId,
                TransportName = transportName,
                NumberOfPackages = transfer.NumberOfPackages,
                DeclaredValue = transfer.DeclaredValue,
                Observations = transfer.Observations,
                UserId = transfer.UserId,
                Articles = articles
            };
        }
        public async Task<IEnumerable<StockTransferDTO>> GetAllAsync()
        {
            var transfers = await _stockTransferRepository.GetAllAsync();
            var result = new List<StockTransferDTO>();

            foreach (var transfer in transfers)
            {
                var articles = new List<StockTransferArticleDTO>();

                foreach (var a in transfer.Articles)
                {
                    var name = await _catalogValidatorService.GetArticleNameAsync(a.ArticleId);
                    articles.Add(new StockTransferArticleDTO
                    {
                        ArticleId = a.ArticleId,
                        ArticleName = name ?? string.Empty,
                        Quantity = a.Quantity
                    });
                }

                // get warehouses name
                var originWarehouseName = await _catalogValidatorService.GetWarehouseNameAsync(transfer.OriginWarehouseId);
                var destinationWarehouseName = await _catalogValidatorService.GetWarehouseNameAsync(transfer.DestinationWarehouseId);

                result.Add(new StockTransferDTO
                {
                    Id = transfer.Id,
                    Code = transfer.Code,
                    Date = transfer.Date,
                    OriginWarehouseId = transfer.OriginWarehouseId,
                    OriginWarehouseName = originWarehouseName,
                    DestinationWarehouseId = transfer.DestinationWarehouseId,
                    DestinationWarehouseName = destinationWarehouseName,
                    TransportId = transfer.TransportId,
                    NumberOfPackages = transfer.NumberOfPackages,
                    DeclaredValue = transfer.DeclaredValue,
                    Observations = transfer.Observations,
                    UserId = transfer.UserId,
                    Articles = articles
                });
            }

            return result;
        }

        

    }
}
