using AuthService.Infrastructure.Models;
using StockService.Business.Interfaces;
using StockService.Business.Models;
using StockService.Business.Models.CommitedStock;
using StockService.Infrastructure.Interfaces;
using StockService.Infrastructure.Models;

namespace StockService.Business.Services
{
    public class CommitedStockService : ICommitedStockService
    {
        private readonly ICommitedStockEntryRepository _commitedStockEntryRepository;
        private readonly IStockMovementRepository _movementRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IStockService _stockService;
        private readonly IStockByDispatchRepository _dispatchRepository;

        public CommitedStockService(
            ICommitedStockEntryRepository commitedStockEntryRepository,
            IStockMovementRepository movementRepository,
            IStockRepository stockRepository,
            IStockService stockService,
            IStockByDispatchRepository dispatchRepository)
        {
            _commitedStockEntryRepository = commitedStockEntryRepository;
            _movementRepository = movementRepository;
            _stockRepository = stockRepository;
            _stockService = stockService;
            _dispatchRepository = dispatchRepository;
        }

        public async Task AddAsync(CommitedStockEntryCreateDTO dto)
        {
            var entity = new CommitedStockEntry
            {
                SaleId = dto.SaleId,
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                ArticleId = dto.ArticleId,
                Quantity = dto.Quantity,
                Delivered = 0 // Initial delivered quantity is 0

            };
            await _commitedStockEntryRepository.AddAsync(entity);
        }

        public async Task<List<CommitedStockEntryDTO>> GetBySaleIdAsync(int saleId)
        {
            var entries = await _commitedStockEntryRepository.GetBySaleIdAsync(saleId);
            return entries.Select(e => new CommitedStockEntryDTO
            {
                Id = e.Id,
                SaleId = e.SaleId,
                CustomerId = e.CustomerId,
                CustomerName = e.CustomerName,
                ArticleId = e.ArticleId,
                Quantity = e.Quantity,
                Delivered = e.Delivered,
                Remaining = e.Remaining
            }).ToList();
        }

        public async Task<RegisterCommitedStockOutputDTO> RegisterCommitedStockAsync(RegisterCommitedStockInputDTO dto, int userId)
        {
            var commitedEntries = await _commitedStockEntryRepository.GetBySaleIdAsync(dto.SaleId);

            //create  RegisterCommitedStockOutputDTO
            var outputDto = new RegisterCommitedStockOutputDTO
            {
                SaleId = dto.SaleId,
                WarehouseId = dto.WarehouseId,
                Reference = dto.Reference,
                Dispatches = new List<RegisterCommitedStockDispatchOutputDTO>()
            };

            foreach (var entry in dto.Articles)
            {
                var existingEntry = commitedEntries.FirstOrDefault(e => e.ArticleId == entry.ArticleId);
                if (existingEntry != null)
                {
                    // create StockMovementCreateDTO and call _stockService.RegisterMovementAsync
                    var movementDto = new StockMovementCreateDTO
                    {
                        ArticleId = entry.ArticleId,
                        Quantity = entry.Quantity,
                        MovementType = StockMovementType.Sale, // Assuming 4 is for Sale
                        FromWarehouseId = dto.WarehouseId,
                        ToWarehouseId = null, // Assuming no specific warehouse for commited stock
                        Reference = $"Commited stock for sale #{dto.SaleId}"
                    };

                    var breakdown = await _stockService.RegisterMovementAsync(movementDto, userId);


                    await _commitedStockEntryRepository.MarkCompletedDeliveryAsync(existingEntry.Id, entry.Quantity);

                    foreach(var b in breakdown)
                    {
                        //if(b.DispatchId == null)
                        //    continue; // Skip if no dispatch ID

                        //si el dispatchId es null, se agrega igualmente



                        outputDto.Dispatches.Add(new RegisterCommitedStockDispatchOutputDTO
                        {
                            DispatchId = b.DispatchId,
                            ArticleId = entry.ArticleId,
                            Quantity = b.Quantity
                        });
                    }
                }

                
            }

            return outputDto;

        }

        public async Task<CommitedStockSummaryByArticleDTO> GetTotalCommitedStockByArticleIdAsync(int articleId)
        {
            var result = new CommitedStockSummaryByArticleDTO();



            result.Total = await _commitedStockEntryRepository.GetTotalCommitedStockByArticleIdAsync(articleId);

            // get the commited stock entries by article with remaining quantity
            var entries = await _commitedStockEntryRepository.GetRemainingByArticleAsync(articleId);

            // summarize the remaining quantities by Customer and asign to the result
            result.Customers = entries
                .GroupBy(e => new { e.CustomerId, e.CustomerName })
                .Select(g => new CommitedStockSummaryByArticleCustomerDTO
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = g.Key.CustomerName,
                    Quantity = g.Sum(e => e.Remaining)
                })
                .ToList();



            return result;
        }
    }
}
