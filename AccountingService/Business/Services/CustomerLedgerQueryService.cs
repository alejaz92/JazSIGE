using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Common;
using AccountingService.Business.Models.Ledger;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Business.Services
{
    public class CustomerLedgerQueryService : ICustomerLedgerQueryService
    {
        private readonly IUnitOfWork _uow;
        public CustomerLedgerQueryService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PagedResult<CustomerLedgerItemDTO>> GetCustomerLedgerAsync(
         int customerId, DateTime? from, DateTime? to,
         LedgerDocumentKind? kind, LedgerDocumentStatus? status,
         int page, int pageSize, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            // --- 1) DOCUMENTOS (cliente, filtros opcionales) ---
            var docsQ = _uow.LedgerDocuments.Query()
                .Where(d => d.PartyType == PartyType.Customer && d.PartyId == customerId);

            

            if (from.HasValue) docsQ = docsQ.Where(d => d.DocumentDate >= from.Value);
            if (to.HasValue) docsQ = docsQ.Where(d => d.DocumentDate <= to.Value.AddDays(1));
            if (kind.HasValue) docsQ = docsQ.Where(d => d.Kind == kind.Value);
            if (status.HasValue) docsQ = docsQ.Where(d => d.Status == status.Value);

            var docs = await docsQ
                .Select(d => new
                {
                    d.Id,
                    d.DocumentDate,
                    d.Kind,
                    d.Status,
                    d.TotalBase,
                    d.Currency,
                    d.FxRate,
                    d.TotalOriginal,
                    FiscalNumber = d.FiscalDocumentNumber
                })
                .ToListAsync(ct);

            // Pendientes por documento = TotalBase - Σ allocations
            var docIds = docs.Select(d => d.Id).ToList();
            var appliedByDoc = docIds.Count == 0
                ? new Dictionary<int, decimal>()
                : await _uow.Allocations.GetAppliedByDocumentsAsync(docIds, ct);

            var docItems = docs.Select(d =>
            {
                appliedByDoc.TryGetValue(d.Id, out var applied);
                var pending = Math.Max(0m, d.TotalBase - applied);
                return new CustomerLedgerItemDTO
                {
                    MovementType = "Document",
                    Id = d.Id,
                    Date = d.DocumentDate,
                    Kind = d.Kind,
                    Status = d.Status,
                    TotalBase = d.TotalBase,
                    PendingBase = pending,
                    Currency = d.Currency,
                    FxRate = d.FxRate,
                    TotalOriginal = d.TotalOriginal,
                    FiscalNumber = d.FiscalNumber
                };
            });

            // --- 2) RECIBOS (cliente, filtros de fecha y "status" mapea a IsVoided) ---
            var recQ = _uow.Receipts.Query()
                .Where(r => r.PartyType == PartyType.Customer && r.PartyId == customerId);

            if (from.HasValue) recQ = recQ.Where(r => r.Date >= from.Value);
            if (to.HasValue) recQ = recQ.Where(r => r.Date <= to.Value);
            if (status.HasValue)
            {
                // Map: Active => !IsVoided, Voided => IsVoided
                recQ = status.Value == LedgerDocumentStatus.Active
                    ? recQ.Where(r => !r.IsVoided)
                    : recQ.Where(r => r.IsVoided);
            }

            var recs = await recQ
                .Select(r => new { r.Id, r.Date, r.TotalBase, r.Currency, r.FxRate, r.TotalOriginal })
                .ToListAsync(ct);

            var receiptIds = recs.Select(r => r.Id).ToList();
            var appliedByReceipt = receiptIds.Count == 0
                ? new Dictionary<int, decimal>()
                : await _uow.Allocations.GetAppliedByReceiptsAsync(receiptIds, ct);

            var recItems = recs.Select(r =>
            {
                appliedByReceipt.TryGetValue(r.Id, out var applied);
                var unapplied = Math.Max(0m, r.TotalBase - applied);
                return new CustomerLedgerItemDTO
                {
                    MovementType = "Receipt",
                    Id = r.Id,
                    Date = r.Date,
                    TotalBase = r.TotalBase,
                    AppliedBase = applied,
                    UnappliedBase = unapplied,
                    Currency = r.Currency,
                    FxRate = r.FxRate,
                    TotalOriginal = r.TotalOriginal
                };
            });

            // --- 3) Unir, ordenar por fecha desc y paginar ---
            var all = docItems.Concat(recItems)
                              .OrderByDescending(x => x.Date)
                              .ThenByDescending(x => x.Id)
                              .ToList();

            var total = all.Count;
            var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<CustomerLedgerItemDTO>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }
    }
}
