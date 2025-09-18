using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Ledger;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Business.Services
{
    public class CustomerPendingQueryService : ICustomerPendingQueryService
    {
        private readonly IUnitOfWork _uow;
        public CustomerPendingQueryService(IUnitOfWork uow) => _uow = uow;

        public async Task<List<PendingDocumentDTO>> GetPendingAsync(int customerId, CancellationToken ct = default)
        {
            // 1) Documentos del cliente, activos y solo débitos (Factura / ND)
            var docs = await _uow.LedgerDocuments.Query()
                .Where(d => d.PartyType == PartyType.Customer
                            && d.PartyId == customerId
                            && d.Status == LedgerDocumentStatus.Active
                            && (d.Kind == LedgerDocumentKind.Invoice || d.Kind == LedgerDocumentKind.DebitNote))
                .Select(d => new
                {
                    d.Id,
                    d.Kind,
                    d.DocumentDate,
                    d.TotalBase,
                    d.FiscalDocumentNumber
                })
                .ToListAsync(ct);

            if (docs.Count == 0) return new List<PendingDocumentDTO>();

            // 2) Suma de imputaciones por documento
            var appliedMap = await _uow.Allocations.GetAppliedByDocumentsAsync(docs.Select(x => x.Id), ct);

            // 3) Calcular pendiente y filtrar > 0
            var result = docs
                .Select(d =>
                {
                    appliedMap.TryGetValue(d.Id, out var applied);
                    var pending = Math.Max(0m, d.TotalBase - applied);
                    return new PendingDocumentDTO
                    {
                        DocumentId = d.Id,
                        Kind = d.Kind,
                        Date = d.DocumentDate,
                        FiscalNumber = d.FiscalDocumentNumber,
                        TotalBase = d.TotalBase,
                        AppliedBase = applied,
                        PendingBase = pending
                    };
                })
                .Where(x => x.PendingBase > 0m)
                .OrderBy(x => x.Date)        // podés cambiar a desc si preferís
                .ThenBy(x => x.DocumentId)
                .ToList();

            return result;
        }
    }
}
