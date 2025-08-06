using SalesService.Business.Models.Clients;

namespace SalesService.Business.Interfaces.Clients
{
    public interface IStockServiceClient
    {
        Task<decimal> GetAvailableStockAsync(int articleId);
        Task RegisterCommitedStockAsync(CommitedStockEntryCreateDTO dto);
        Task<CommitedStockEntryOutputDTO> RegisterCommitedStockConsolidatedAsync(CommitedStockInputDTO dto, int userId);
    }
}
