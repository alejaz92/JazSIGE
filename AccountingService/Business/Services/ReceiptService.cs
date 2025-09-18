using AccountingService.Business.Models.Ledger;
using AccountingService.Business.Services.Interfaces;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models.Ledger;

namespace AccountingService.Business.Services;

public class ReceiptService : IReceiptService
{
    private readonly IUnitOfWork _uow;

    public ReceiptService(IUnitOfWork uow) => _uow = uow;

    public async Task<ReceiptResponseDTO> CreateAsync(ReceiptRequestCreateDTO req, CancellationToken ct = default)
    {
        if (req.PartyType != PartyType.Customer)
            throw new InvalidOperationException("Only Customer is allowed in V1.");

        if (req.Payments is null || req.Payments.Count == 0)
            throw new InvalidOperationException("Receipt must contain at least one payment.");

        // Validaciones de banco para transferencia/depósito
        foreach (var p in req.Payments)
        {
            if ((p.Method == PaymentMethod.BankTransfer || p.Method == PaymentMethod.BankDeposit) && p.BankAccountId is null)
                throw new InvalidOperationException("BankAccountId is required for bank transfer/deposit.");
        }

        // Map a entidad
        var receipt = new Receipt
        {
            Number = null, // numeración interna la vemos luego
            Date = req.Date,
            PartyType = req.PartyType,
            PartyId = req.PartyId,
            Currency = req.Currency,
            FxRate = req.FxRate,
            Notes = req.Notes,
            IsVoided = false
        };

        decimal totalOriginal = 0m, totalBase = 0m;

        receipt.PaymentLines = req.Payments.Select(p =>
        {
            var amountBase = Math.Round(p.AmountOriginal * req.FxRate, 2, MidpointRounding.ToEven);
            totalOriginal += p.AmountOriginal;
            totalBase += amountBase;

            return new PaymentLine
            {
                Method = p.Method,
                AmountOriginal = p.AmountOriginal,
                AmountBase = amountBase,
                BankAccountId = p.BankAccountId,
                TransactionReference = p.TransactionReference,
                Notes = p.Notes,
                ValueDate = p.ValueDate
            };
        }).ToList();

        receipt.TotalOriginal = totalOriginal;
        receipt.TotalBase = totalBase;

        await _uow.BeginTransactionAsync(ct);
        try
        {
            await _uow.Receipts.AddAsync(receipt);
            await _uow.SaveChangesAsync(ct);

            if (req.Allocations is not null && req.Allocations.Count > 0)
            {
                // 1) Traer documentos y validar pertenencia/tipos
                var docIds = req.Allocations.Select(a => a.DocumentId).Distinct().ToList();
                var docs = await _uow.LedgerDocuments.FindAsync(d => docIds.Contains(d.Id));
                var docsById = docs.ToDictionary(d => d.Id);

                foreach (var a in req.Allocations)
                {
                    if (!docsById.TryGetValue(a.DocumentId, out var doc))
                        throw new InvalidOperationException($"Document {a.DocumentId} not found.");

                    if (doc.PartyType != receipt.PartyType || doc.PartyId != receipt.PartyId)
                        throw new InvalidOperationException($"Document {a.DocumentId} belongs to another party.");

                    if (doc.Kind == LedgerDocumentKind.CreditNote)
                        throw new InvalidOperationException("Credit notes are not allocated via receipt.");
                }

                // 2) Validar pendientes actuales por documento
                var applied = await _uow.Allocations.GetAppliedByDocumentsAsync(docIds, ct);
                foreach (var a in req.Allocations)
                {
                    var doc = docsById[a.DocumentId];
                    var already = applied.TryGetValue(a.DocumentId, out var sum) ? sum : 0m;
                    var pending = doc.TotalBase - already;

                    if (a.AmountBase > pending + 0.0001m)
                        throw new InvalidOperationException($"Allocation exceeds pending for document {a.DocumentId}. Pending: {pending:N2}");
                }

                // 3) Validar que la suma no exceda el total del recibo
                var sumAlloc = req.Allocations.Sum(x => x.AmountBase);
                if (sumAlloc > receipt.TotalBase + 0.0001m)
                    throw new InvalidOperationException("Allocations exceed receipt total.");

                // 4) Crear imputaciones
                foreach (var a in req.Allocations)
                {
                    await _uow.Allocations.AddAsync(new Allocation
                    {
                        ReceiptId = receipt.Id,
                        DebitDocumentId = a.DocumentId,
                        AmountBase = Math.Round(a.AmountBase, 2, MidpointRounding.ToEven)
                    });
                }

                await _uow.SaveChangesAsync(ct);
            }

            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }

        return Map(receipt);
    }

    private static ReceiptResponseDTO Map(Receipt r) => new()
    {
        Id = r.Id,
        Number = r.Number,
        Date = r.Date,
        PartyType = r.PartyType,
        PartyId = r.PartyId,
        Currency = r.Currency,
        FxRate = r.FxRate,
        TotalOriginal = r.TotalOriginal,
        TotalBase = r.TotalBase,
        Notes = r.Notes,
        IsVoided = r.IsVoided,
        VoidedAt = r.VoidedAt,
        Payments = r.PaymentLines.Select(p => new PaymentLineResponse
        {
            Id = p.Id,
            Method = p.Method,
            AmountOriginal = p.AmountOriginal,
            AmountBase = p.AmountBase,
            BankAccountId = p.BankAccountId,
            TransactionReference = p.TransactionReference,
            Notes = p.Notes,
            ValueDate = p.ValueDate
        }).ToList()
    };
}
