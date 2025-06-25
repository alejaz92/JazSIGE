using SalesService.Business.Models.Clients;

namespace SalesService.Business.Interfaces.Clients
{
    public interface IStockServiceClient
    {
        Task RegisterCommitedStockAsync(CommitedStockEntryCreateDTO dto);
        Task<CommitedStockEntryOutputDTO> RegisterCommitedStockConsolidatedAsync(CommitedStockInputDTO dto, int userId);
    }
}
