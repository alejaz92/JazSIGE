using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Ledger;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Business.Services
{
    public class CustomerBalancesQueryService : ICustomerBalancesQueryService
    {
        private readonly IUnitOfWork _uow;
        public CustomerBalancesQueryService(IUnitOfWork uow) => _uow = uow;

        public async Task<CustomerBalancesDTO> GetBalancesAsync(int customerId, CancellationToken ct = default)
        {
            // --- 1) Deuda abierta: FAC + ND (solo documentos activos) ---
            var debitDocs = await _uow.LedgerDocuments.Query()
                .Where(d => d.PartyType == PartyType.Customer
                         && d.PartyId == customerId
                         && d.Status == LedgerDocumentStatus.Active
                         && (d.Kind == LedgerDocumentKind.Invoice || d.Kind == LedgerDocumentKind.DebitNote))
                .Select(d => new { d.Id, d.TotalBase })
                .ToListAsync(ct);

            var appliedToDebits = debitDocs.Count == 0
                ? new Dictionary<int, decimal>()
                : await _uow.Allocations.GetAppliedByDocumentsAsync(debitDocs.Select(x => x.Id), ct);

            var outstanding = debitDocs.Sum(d =>
            {
                appliedToDebits.TryGetValue(d.Id, out var applied);
                var pending = d.TotalBase - applied;
                return pending > 0 ? decimal.Round(pending, 2, MidpointRounding.ToEven) : 0m;
            });

            // --- 2) Créditos: (a) Notas de crédito activas + (b) anticipos (recibos con saldo sin imputar) ---
            // (a) NC completas como crédito disponible (luego descontaremos cuando implementemos su aplicación)
            var creditNotes = await _uow.LedgerDocuments.Query()
                .Where(d => d.PartyType == PartyType.Customer
                         && d.PartyId == customerId
                         && d.Status == LedgerDocumentStatus.Active
                         && d.Kind == LedgerDocumentKind.CreditNote)
                .SumAsync(d => (decimal?)d.TotalBase, ct) ?? 0m;

            // (b) Anticipos = total del recibo - aplicado; solo recibos no anulados
            var receipts = await _uow.Receipts.Query()
                .Where(r => r.PartyType == PartyType.Customer
                         && r.PartyId == customerId
                         && !r.IsVoided)
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

            return new CustomerBalancesDTO
            {
                OutstandingArs = outstanding,
                CreditsArs = credits,
                NetBalanceArs = net
            };
        }
    }
 }
