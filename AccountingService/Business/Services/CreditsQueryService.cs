using AccountingService.Business.Interfaces;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.EntityFrameworkCore;
using static AccountingService.Business.Models.Ledger.CreditItemsAndApplyDTO;

namespace AccountingService.Business.Services
{
    public class CreditsQueryService : ICreditsQueryService
    {
        private readonly IUnitOfWork _uow;
        public CreditsQueryService(IUnitOfWork uow) => _uow = uow;

        public async Task<IReadOnlyList<CreditItemDTO>> GetAvailableAsync(
            int customerId, 
            decimal? minAmountBase = null,
            CancellationToken ct = default)
        {
            // NC activas del cliente
            var creditDocs = await _uow.LedgerDocuments.Query()
                .Where(d => d.PartyType == PartyType.Customer
                         && d.PartyId == customerId
                         && d.Status == LedgerDocumentStatus.Active
                         && d.Kind == LedgerDocumentKind.CreditNote)
                .Select(d => new { d.Id, d.DocumentDate, d.DisplayNumber, d.TotalBase })
                .ToListAsync(ct);

            var appliedByCreditDoc = creditDocs.Count == 0
                ? new Dictionary<int, decimal>()
                : await _uow.Allocations.GetAppliedByCreditDocsAsync(creditDocs.Select(x => x.Id), ct); // :contentReference[oaicite:3]{index=3}

            var ncItems = creditDocs
                .Select(cd =>
                {
                    appliedByCreditDoc.TryGetValue(cd.Id, out var applied);
                    var available = cd.TotalBase - applied;
                    return new CreditItemDTO
                    {
                        Kind = "creditNote",
                        Id = cd.Id,
                        Date = cd.DocumentDate,
                        Number = cd.DisplayNumber,
                        AvailableBase = available > 0 ? Math.Round(available, 2, MidpointRounding.ToEven) : 0m
                    };
                })
                .Where(x => x.AvailableBase > 0m);

            // Recibos no anulados del cliente
            var receipts = await _uow.Receipts.Query()
                .Where(r => r.PartyType == PartyType.Customer && r.PartyId == customerId && !r.IsVoided)
                .Select(r => new { r.Id, r.Date, Number = (string?)null, r.TotalBase })
                .ToListAsync(ct);

            var appliedByReceipt = receipts.Count == 0
                ? new Dictionary<int, decimal>()
                : await _uow.Allocations.GetAppliedByReceiptsAsync(receipts.Select(x => x.Id), ct); // :contentReference[oaicite:4]{index=4}

            var receiptItems = receipts
                .Select(r =>
                {
                    appliedByReceipt.TryGetValue(r.Id, out var applied);
                    var available = r.TotalBase - applied;
                    return new CreditItemDTO
                    {
                        Kind = "receipt",
                        Id = r.Id,
                        Date = r.Date,
                        Number = null,
                        AvailableBase = available > 0 ? Math.Round(available, 2, MidpointRounding.ToEven) : 0m
                    };
                })
                .Where(x => x.AvailableBase > 0m);

            var all = ncItems.Concat(receiptItems).OrderBy(x => x.Date).ThenBy(x => x.Id).ToList();

            if (minAmountBase.HasValue)
            {
                var sum = all.Sum(x => x.AvailableBase);
                if (sum < minAmountBase.Value) return Array.Empty<CreditItemDTO>();
            }

            return all;

        }
    }
}
