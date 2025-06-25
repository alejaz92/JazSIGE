using StockService.Business.Models;
using StockService.Business.Models.CommitedStock;

namespace StockService.Business.Interfaces
{
    public interface ICommitedStockService
    {
        Task AddAsync(CommitedStockEntryCreateDTO dto);
        Task<List<CommitedStockEntryDTO>> GetBySaleIdAsync(int saleId);
        Task<decimal> GetTotalCommitedStockByArticleIdAsync(int articleId);
        Task<RegisterCommitedStockOutputDTO> RegisterCommitedStockAsync(RegisterCommitedStockInputDTO dto, int userId);
    }
}
