using AccountingService.Infrastructure.Data;
using AccountingService.Infrastructure.Models;
using JazSIGE.Accounting.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static AccountingService.Infrastructure.Models.Enums;

namespace AccountingService.Infrastructure.Repositories
{
    public class LedgerDocumentRepository : GenericRepository<LedgerDocument>, ILedgerDocumentRepository
    {
        public LedgerDocumentRepository(AccountingDbContext ctx) : base(ctx) { }

        public async Task<LedgerDocument?> GetByExternalRefAsync(int externalRefId, LedgerDocumentKind kind, PartyType partyType)
        {
            return await _db.FirstOrDefaultAsync(x => x.ExternalRefId == externalRefId && x.Kind == kind && x.PartyType == partyType);
        }

        public async Task<List<LedgerDocument>> GetPartyDocumentsAsync(PartyType partyType, int partyId)
        {
            return await _db
                .Where(x => x.PartyType == partyType && x.PartyId == partyId && x.Status == DocumentStatus.Active)
                .OrderByDescending(x => x.DocumentDate)
                .ToListAsync();
        }

        public async Task<List<LedgerDocument>> GetSelectablesDebitsAsync(PartyType partyType, int partyId)
        {
            return await _db
                .Where(x =>
                    x.PartyType == partyType &&
                    x.PartyId == partyId &&
                    x.Status == DocumentStatus.Active &&
                    (x.Kind == LedgerDocumentKind.Invoice || x.Kind == LedgerDocumentKind.DebitNote) &&
                    x.PendingARS > 0)
                .OrderBy(x => x.DocumentDate)
                .ToListAsync();
        }

        public async Task<List<LedgerDocument>> GetSelectablesCreditsAsync(PartyType partyType, int partyId)
        {
            return await _db
                .Where(x =>
                    x.PartyType == partyType &&
                    x.PartyId == partyId &&
                    x.Status == DocumentStatus.Active &&
                    x.Kind == LedgerDocumentKind.CreditNote &&
                    x.PendingARS > 0)
                .OrderBy(x => x.DocumentDate)
                .ToListAsync();
        }

        public async Task<List<LedgerDocument>> GetReceiptCreditsAsync(PartyType partyType, int partyId)
        {
            return await _db
                .Where(x =>
                    x.PartyType == partyType &&
                    x.PartyId == partyId &&
                    x.Status == DocumentStatus.Active &&
                    x.Kind == LedgerDocumentKind.Receipt &&
                    x.PendingARS > 0)
                .OrderBy(x => x.DocumentDate)
                .ToListAsync();
        }
    }
}
