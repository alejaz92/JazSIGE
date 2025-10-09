using AccountingService.Business.Models;
using static AccountingService.Infrastructure.Models.Enums;

namespace AccountingService.Business.Interfaces
{
    public interface ILedgerQueryService
    {
        Task<BalancesDTO> GetBalancesAsync(PartyType partyType, int partyId);
        Task<PagedResult<LedgerDocumentDTO>> GetLedgerAsync(PartyType partyType, int partyId,
            DateTime? from, DateTime? to, LedgerDocumentKind? kind, DocumentStatus? status,
            int page, int pageSize);

        // Wizard del recibo
        Task<SelectablesDTO> GetSelectablesForReceiptAsync(PartyType partyType, int partyId);

        // Modal “cover-invoice”: SOLO recibos con saldo
        Task<List<SimpleDocDTO>> GetReceiptCreditsAsync(PartyType partyType, int partyId);
    }
}
