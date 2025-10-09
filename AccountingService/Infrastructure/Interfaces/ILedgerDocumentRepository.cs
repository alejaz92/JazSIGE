using AccountingService.Infrastructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AccountingService.Infrastructure.Models.Enums;

namespace JazSIGE.Accounting.Infrastructure.Interfaces
{
    public interface ILedgerDocumentRepository : IGenericRepository<LedgerDocument>
    {
        Task<LedgerDocument?> GetByExternalRefAsync(int externalRefId, LedgerDocumentKind kind);
        Task<List<LedgerDocument>> GetPartyDocumentsAsync(PartyType partyType, int partyId);

        // Selectables (wizard recibo)
        Task<List<LedgerDocument>> GetSelectablesDebitsAsync(PartyType partyType, int partyId);   // Invoice + DebitNote (Active, Pending>0)
        Task<List<LedgerDocument>> GetSelectablesCreditsAsync(PartyType partyType, int partyId);  // CreditNote (Active, Pending>0)
        Task<List<LedgerDocument>> GetReceiptCreditsAsync(PartyType partyType, int partyId);      // Receipts with Pending>0
    }
}
