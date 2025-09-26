using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Common;
using AccountingService.Business.Models.Ledger;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Business.Services
{
    public class CustomerAccountQueryService : ICustomerAccountQueryService
    {
        private readonly IUnitOfWork _uow;

        public CustomerAccountQueryService(IUnitOfWork uow) => _uow = uow;

        // Ledger unificado: TODO sale de Documents (Factura/ND/NC/Recibo)
        public async Task<PagedResult<LedgerItemDTO>> GetLedgerAsync(
            int customerId,
            DateTime? from,
            DateTime? to,
            LedgerDocumentKind? kind,
            LedgerDocumentStatus? status,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            var q = _uow.LedgerDocuments.Query()
                .Where(d => d.PartyType == PartyType.Customer && d.PartyId == customerId);

            if (from.HasValue) q = q.Where(d => d.DocumentDate >= from.Value.Date);
            if (to.HasValue) q = q.Where(d => d.DocumentDate < to.Value.Date.AddDays(1));

            if (kind.HasValue) q = q.Where(d => d.Kind == kind.Value);
            if (status.HasValue) q = q.Where(d => d.Status == status.Value);

            q = q.OrderByDescending(d => d.DocumentDate).ThenByDescending(d => d.Id);

            var total = await q.CountAsync(ct);

            var items = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new LedgerItemDTO
                {
                    DocumentId = d.Id,
                    Kind = d.Kind,
                    Status = d.Status,
                    Date = d.DocumentDate,
                    Number = d.DisplayNumber,     // neutro (factura/nd/nc/recibo)
                    Currency = d.Currency,
                    FxRate = d.FxRate,
                    TotalOriginal = d.TotalOriginal,
                    TotalBase = d.TotalBase
                })
                .ToListAsync(ct);

            return new PagedResult<LedgerItemDTO>(items, total, page, pageSize);
        }

        // Pendientes: solo débitos activos (Factura/ND) con saldo > 0
        public async Task<PagedResult<PendingItemDTO>> GetPendingAsync(
            int customerId,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 500) pageSize = 100;

            var debitsQ = _uow.LedgerDocuments.Query()
                .Where(d => d.PartyType == PartyType.Customer
                         && d.PartyId == customerId
                         && d.Status == LedgerDocumentStatus.Active
                         && (d.Kind == LedgerDocumentKind.Invoice || d.Kind == LedgerDocumentKind.DebitNote));

            var total = await debitsQ.CountAsync(ct);

            var debits = await debitsQ
                .OrderBy(d => d.DocumentDate).ThenBy(d => d.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new { d.Id, d.Kind, d.DocumentDate, d.DisplayNumber, d.TotalBase })
                .ToListAsync(ct);

            var ids = debits.Select(d => d.Id).ToArray();

            // Ya lo usabas en tus servicios actuales: suma de imputaciones por documento. :contentReference[oaicite:4]{index=4}
            var appliedByDoc = ids.Length == 0
                ? new Dictionary<int, decimal>()
                : await _uow.Allocations.GetAppliedByDocumentsAsync(ids, ct);

            var items = debits.Select(d =>
            {
                appliedByDoc.TryGetValue(d.Id, out var applied);
                var pending = Math.Max(0m, d.TotalBase - applied);

                return new PendingItemDTO
                {
                    DocumentId = d.Id,
                    Kind = d.Kind,
                    Date = d.DocumentDate,
                    Number = d.DisplayNumber,
                    TotalBase = d.TotalBase,
                    Applied = applied,
                    Pending = pending
                };
            }).Where(x => x.Pending > 0m).ToList();

            return new PagedResult<PendingItemDTO>(items, total, page, pageSize);
        }

        // Balances: Outstanding (pendiente en débitos), Credits (NC + recibos no aplicados), Net = Outstanding - Credits
        public async Task<BalancesDTO> GetBalancesAsync(int customerId, CancellationToken ct = default)
        {
            // 1) Débitos activos (Factura/ND)
            var debits = await _uow.LedgerDocuments.Query()
                .Where(d => d.PartyType == PartyType.Customer
                         && d.PartyId == customerId
                         && d.Status == LedgerDocumentStatus.Active
                         && (d.Kind == LedgerDocumentKind.Invoice || d.Kind == LedgerDocumentKind.DebitNote))
                .Select(d => new { d.Id, d.TotalBase })
                .ToListAsync(ct);

            var appliedByDoc = debits.Count == 0
                ? new Dictionary<int, decimal>()
                : await _uow.Allocations.GetAppliedByDocumentsAsync(debits.Select(x => x.Id), ct);

            var outstanding = debits.Sum(d =>
            {
                appliedByDoc.TryGetValue(d.Id, out var applied);
                var pending = d.TotalBase - applied;
                return pending > 0 ? decimal.Round(pending, 2, MidpointRounding.ToEven) : 0m;
            });

            // 2) Créditos: (a) NC activas + (b) recibos no aplicados
            var creditNotes = await _uow.LedgerDocuments.Query()
                .Where(d => d.PartyType == PartyType.Customer
                         && d.PartyId == customerId
                         && d.Status == LedgerDocumentStatus.Active
                         && d.Kind == LedgerDocumentKind.CreditNote)
                .SumAsync(d => (decimal?)d.TotalBase, ct) ?? 0m;

            // Recibos no aplicados: como ya hacías, miramos Receipt + allocations por ReceiptId. :contentReference[oaicite:5]{index=5}
            var receipts = await _uow.Receipts.Query()
                .Where(r => r.PartyType == PartyType.Customer && r.PartyId == customerId && !r.IsVoided)
                .Select(r => new { r.Id, r.TotalBase })
                .ToListAsync(ct);

            var appliedByReceipt = receipts.Count == 0
                ? new Dictionary<int, decimal>()
                : await _uow.Allocations.GetAppliedByReceiptsAsync(receipts.Select(x => x.Id), ct);

            var advances = receipts.Sum(r =>
            {
                appliedByReceipt.TryGetValue(r.Id, out var applied);
                var unapplied = r.TotalBase - applied;
                return unapplied > 0 ? decimal.Round(unapplied, 2, MidpointRounding.ToEven) : 0m;
            });

            var credits = decimal.Round(creditNotes + advances, 2, MidpointRounding.ToEven);
            var net = decimal.Round(outstanding - credits, 2, MidpointRounding.ToEven);

            return new BalancesDTO
            {
                OutstandingArs = outstanding,
                CreditsArs = credits,
                NetBalanceArs = net
            };
        }
    }
}
