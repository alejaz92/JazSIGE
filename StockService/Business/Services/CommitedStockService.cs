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
        private readonly IStockService _stockService;

        public CommitedStockService(
            ICommitedStockEntryRepository commitedStockEntryRepository,
            IStockService stockService)
        {
            _commitedStockEntryRepository = commitedStockEntryRepository;
            _stockService = stockService;
        }

        public async Task AddAsync(CommitedStockEntryCreateDTO dto)
        {
            var entity = new CommitedStockEntry
            {
                SaleId = dto.SaleId,
                IsFinalConsumer = dto.IsFinalConsumer,
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
                IsFinalConsumer = e.IsFinalConsumer,
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



            //create  RegisterCommitedStockOutputDTO
            var outputDto = new RegisterCommitedStockOutputDTO
            {
                SaleId = dto.SaleId,
                WarehouseId = dto.WarehouseId,
                Reference = dto.Reference,
                Dispatches = new List<RegisterCommitedStockDispatchOutputDTO>()
            };

            if (!dto.IsQuick)
            {
                var commitedEntries = await _commitedStockEntryRepository.GetBySaleIdAsync(dto.SaleId);

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
                            Reference = $"Stock comprometido por venta #{dto.SaleId}"
                        };

                        var breakdown = await _stockService.RegisterMovementAsync(movementDto, userId);


                        await _commitedStockEntryRepository.MarkCompletedDeliveryAsync(existingEntry.Id, entry.Quantity);

                        foreach (var b in breakdown)
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
            }
            else
            {
                foreach (var entry in dto.Articles)
                {

                    // check stock availability
                    var stockAvailable = await _stockService.GetStockAsync(entry.ArticleId, dto.WarehouseId);
                    if (stockAvailable < entry.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for article {entry.ArticleId} in warehouse {dto.WarehouseId}. Available: {stockAvailable}, Required: {entry.Quantity}");


                    // create StockMovementCreateDTO and call _stockService.RegisterMovementAsync
                    var movementDto = new StockMovementCreateDTO
                    {
                        ArticleId = entry.ArticleId,
                        Quantity = entry.Quantity,
                        MovementType = StockMovementType.Sale, // Assuming 4 is for Sale
                        FromWarehouseId = dto.WarehouseId,
                        ToWarehouseId = null, // Assuming no specific warehouse for commited stock
                        Reference = $"Movimiento de Stock para venta rapida #{dto.SaleId}"
                    };

                    var breakdown = await _stockService.RegisterMovementAsync(movementDto, userId);


                    foreach (var b in breakdown)
                    {


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

        // function to update commited stock entry (only reduction of quantity is allowed) receives saleId, articleId and new quantity
        public async Task UpdateCommitedStockEntryAsync(CommitedStockEntryUpdateDTO dto)
        {
            var existingEntries = await _commitedStockEntryRepository.GetBySaleIdAsync(dto.SaleId);
            foreach (var articleUpdate in dto.Articles)
            {
                var existingEntry = existingEntries.FirstOrDefault(e => e.ArticleId == articleUpdate.ArticleId);
                if (existingEntry != null)
                {
                    if (articleUpdate.NewQuantity < existingEntry.Quantity)
                    {
                        await _commitedStockEntryRepository.UpdateQuantityAsync(existingEntry.Id, articleUpdate.NewQuantity);
                    }
                    else
                    {
                        throw new InvalidOperationException("Only reduction of quantity is allowed.");
                    }
                }
                
            }

        }
    }
}
