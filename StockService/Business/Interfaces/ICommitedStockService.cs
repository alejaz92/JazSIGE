using StockService.Business.Models;
using StockService.Business.Models.CommitedStock;
using StockService.Infrastructure.Models;

namespace StockService.Business.Interfaces
{
    public interface ICommitedStockService
    {
        Task AddAsync(CommitedStockEntryCreateDTO dto);
        Task<List<CommitedStockEntryDTO>> GetBySaleIdAsync(int saleId);
        Task<CommitedStockSummaryByArticleDTO> GetTotalCommitedStockByArticleIdAsync(int articleId);
        Task<RegisterCommitedStockOutputDTO> RegisterCommitedStockAsync(RegisterCommitedStockInputDTO dto, int userId);
        Task UpdateCommitedStockEntryAsync(CommitedStockEntryUpdateDTO dto);
    }
}
