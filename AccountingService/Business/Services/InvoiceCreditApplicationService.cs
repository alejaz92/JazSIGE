using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Ledger;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.EntityFrameworkCore;
using static AccountingService.Business.Models.Ledger.CreditItemsAndApplyDTO;

namespace AccountingService.Business.Services
{
    public sealed class InvoiceCreditApplicationService : IInvoiceCreditApplicationService
    {
        private sealed record ReceiptLite(
            int Id, int PartyId, bool IsVoided, decimal TotalBase);

        private sealed record CreditDocLite(
            int Id, int PartyId, LedgerDocumentStatus Status, LedgerDocumentKind Kind, decimal TotalBase);

        private readonly IUnitOfWork _uow;
        public InvoiceCreditApplicationService(IUnitOfWork uow) => _uow = uow;

        public async Task<ApplyCreditsResult> ApplyAsync(ApplyCreditsRequest req, CancellationToken ct = default)
        {
            // 1) Factura/ND a cubrir (activa, del cliente, con pendiente > 0)
            var debit = await _uow.LedgerDocuments.Query()
                .FirstOrDefaultAsync(d => d.Id == req.InvoiceId, ct)
                ?? throw new InvalidOperationException($"Document {req.InvoiceId} not found.");

            if (debit.PartyType != PartyType.Customer || debit.PartyId != req.CustomerId)
                throw new InvalidOperationException("Invoice and customer mismatch.");

            if (debit.Status != LedgerDocumentStatus.Active)
                throw new InvalidOperationException("Invoice is not active.");

            if (debit.Kind != LedgerDocumentKind.Invoice && debit.Kind != LedgerDocumentKind.DebitNote)
                throw new InvalidOperationException("Only Invoice or Debit Note can receive credit application.");

            var appliedToDebitDict = await _uow.Allocations.GetAppliedByDocumentsAsync(new[] { debit.Id }, ct); // :contentReference[oaicite:6]{index=6}
            var appliedToDebit = appliedToDebitDict.TryGetValue(debit.Id, out var sumD) ? sumD : 0m;
            var pending = debit.TotalBase - appliedToDebit;
            if (pending <= 0m) return new ApplyCreditsResult { InvoiceId = debit.Id, AppliedTotalBase = 0m, Splits = Array.Empty<AppliedSplit>() };

            // 2) Traer créditos seleccionados (orden explícito)
            var pickReceipts = req.Items.Where(i => i.Kind == "receipt").Select(i => i.Id).Distinct().ToList();
            var pickCredits = req.Items.Where(i => i.Kind == "creditNote").Select(i => i.Id).Distinct().ToList();

            // Recibos elegidos
            var receipts = pickReceipts.Count == 0
                ? new List<ReceiptLite>()
                : await _uow.Receipts.Query()
                    .Where(r => pickReceipts.Contains(r.Id))
                    .Select(r => new ReceiptLite(r.Id, r.PartyId, r.IsVoided, r.TotalBase))
                    .ToListAsync(ct);

            if (receipts.Count != pickReceipts.Count)
                throw new InvalidOperationException("One or more receipts not found.");

            // NC elegidas
            var creditDocs = pickCredits.Count == 0
                ? new List<CreditDocLite>()
                : await _uow.LedgerDocuments.Query()
                    .Where(d => pickCredits.Contains(d.Id))
                    .Select(d => new CreditDocLite(d.Id, d.PartyId, d.Status, d.Kind, d.TotalBase))
                    .ToListAsync(ct);

            if (creditDocs.Count != pickCredits.Count)
                throw new InvalidOperationException("One or more credit notes not found.");


            // Validaciones
            foreach (var r in receipts)
            {
                if (r.PartyId != req.CustomerId) throw new InvalidOperationException($"Receipt {r.Id} belongs to another customer.");
                if (r.IsVoided) throw new InvalidOperationException($"Receipt {r.Id} is voided.");
            }
            foreach (var c in creditDocs)
            {
                if (c.PartyId != req.CustomerId) throw new InvalidOperationException($"Credit note {c.Id} belongs to another customer.");
                if (c.Status != LedgerDocumentStatus.Active) throw new InvalidOperationException($"Credit note {c.Id} is not active.");
                if (c.Kind != LedgerDocumentKind.CreditNote) throw new InvalidOperationException($"Document {c.Id} is not a credit note.");
            }

            // Disponibles
            var appliedByReceipt = receipts.Count == 0
                ? new Dictionary<int, decimal>()
                : await _uow.Allocations.GetAppliedByReceiptsAsync(receipts.Select(x => x.Id), ct);

            var receiptAvail = receipts.ToDictionary(
                r => r.Id,
                r => Math.Round(r.TotalBase - (appliedByReceipt.TryGetValue(r.Id, out var ap) ? ap : 0m), 2, MidpointRounding.ToEven));

            var appliedByCreditDoc = creditDocs.Count == 0
                ? new Dictionary<int, decimal>()
                : await _uow.Allocations.GetAppliedByCreditDocsAsync(creditDocs.Select(x => x.Id), ct);

            var creditDocAvail = creditDocs.ToDictionary(
                c => c.Id,
                c => Math.Round(c.TotalBase - (appliedByCreditDoc.TryGetValue(c.Id, out var ap) ? ap : 0m), 2, MidpointRounding.ToEven));


            // 5) Chequeo: suma de disponibles >= pendiente
            var totalAvail = receiptAvail.Values.Sum() + creditDocAvail.Values.Sum();
            if (totalAvail + 0.0001m < pending)
                throw new InvalidOperationException($"Selected credits are not enough to cover the invoice. Pending: {pending:N2}, Selected: {totalAvail:N2}");

            // 6) Aplicar en el orden recibido
            var splits = new List<AppliedSplit>();
            var remaining = Math.Round(pending, 2, MidpointRounding.ToEven);

            using var tx = await _uow.BeginTransactionAsync(ct);

            foreach (var item in req.Items)
            {
                if (remaining <= 0m) break;

                if (item.Kind == "receipt")
                {
                    if (!receiptAvail.TryGetValue(item.Id, out var avail) || avail <= 0m) continue;
                    var take = Math.Min(avail, remaining);

                    await _uow.Allocations.AddAsync(new Allocation
                    {
                        Source = AllocationSource.Receipt,
                        ReceiptId = item.Id,
                        DebitDocumentId = debit.Id,
                        AmountBase = take,
                        CreatedAt = DateTime.UtcNow
                    });

                    receiptAvail[item.Id] = Math.Round(avail - take, 2, MidpointRounding.ToEven);
                    remaining = Math.Round(remaining - take, 2, MidpointRounding.ToEven);

                    splits.Add(new AppliedSplit { Kind = "receipt", Id = item.Id, AmountBase = take });
                }
                else if (item.Kind == "creditNote")
                {
                    if (!creditDocAvail.TryGetValue(item.Id, out var avail) || avail <= 0m) continue;
                    var take = Math.Min(avail, remaining);

                    await _uow.Allocations.AddAsync(new Allocation
                    {
                        Source = AllocationSource.CreditDocument,
                        CreditDocumentId = item.Id,
                        DebitDocumentId = debit.Id,
                        AmountBase = take,
                        CreatedAt = DateTime.UtcNow
                    });

                    creditDocAvail[item.Id] = Math.Round(avail - take, 2, MidpointRounding.ToEven);
                    remaining = Math.Round(remaining - take, 2, MidpointRounding.ToEven);

                    splits.Add(new AppliedSplit { Kind = "creditNote", Id = item.Id, AmountBase = take });
                }
            }

            await _uow.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            var appliedTotal = splits.Sum(s => s.AmountBase);
            return new ApplyCreditsResult
            {
                InvoiceId = debit.Id,
                AppliedTotalBase = appliedTotal,
                Splits = splits
            };
        }
    }
}
