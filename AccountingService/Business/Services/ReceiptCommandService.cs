using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Ledger;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Business.Services
{
    public class ReceiptCommandService : IReceiptCommandService
    {
        private readonly IUnitOfWork _uow;

        public ReceiptCommandService(IUnitOfWork uow) => _uow = uow;

        public async Task<ReceiptDTO> CreateReceiptAsync(ReceiptCreateDTO req, CancellationToken ct = default)
        {
            if (req.PartyType != PartyType.Customer)
                throw new InvalidOperationException("Only Customer is allowed in V1.");

            if (req.Payments is null || req.Payments.Count == 0)
                throw new InvalidOperationException("Receipt must contain at least one payment.");

            foreach (var p in req.Payments)
            {
                if ((p.Method == PaymentMethod.BankTransfer || p.Method == PaymentMethod.BankDeposit) && p.BankAccountId is null)
                    throw new InvalidOperationException("BankAccountId is required for bank transfer/deposit.");

                if (p.Method == PaymentMethod.Check)
                {
                    if (string.IsNullOrWhiteSpace(p.CheckIssuerBankCode))
                        throw new InvalidOperationException("CheckIssuerBankCode is required for check.");
                    if (string.IsNullOrWhiteSpace(p.CheckNumber))
                        throw new InvalidOperationException("CheckNumber is required for check.");
                    if (p.CheckIssueDate is null)
                        throw new InvalidOperationException("CheckIssueDate is required for check.");
                    if (p.CheckPaymentDate is null)
                        throw new InvalidOperationException("CheckPaymentDate is required for check.");
                    if (p.CheckIssueDate > p.CheckPaymentDate)
                        throw new InvalidOperationException("CheckPaymentDate must be on/after CheckIssueDate.");
                }
            }

            // Punto fijo por ahora
            const string pointOfReceipt = "0001";
            var scope = $"Receipt:{pointOfReceipt}";

            // Nro robusto bajo concurrencia
            var next = await _uow.NumberingSequences.GetNextAsync(scope, ct);
            var formatted = $"{next:D8}";

            var receipt = new Receipt
            {
                Number = formatted,
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

                    // Cheque (solo si corresponde)
                    CheckIssuerBankCode = p.CheckIssuerBankCode,
                    CheckNumber = p.CheckNumber,
                    CheckIssueDate = p.CheckIssueDate,
                    CheckPaymentDate = p.CheckPaymentDate,
                    CheckIssuerTaxId = p.CheckIssuerTaxId,
                    CheckIssuerName = p.CheckIssuerName,
                    CheckIsThirdParty = p.CheckIsThirdParty,

                    // Fecha valor: si no vino y es cheque, usamos fecha de pago
                    ValueDate = p.ValueDate ?? (p.Method == PaymentMethod.Check ? p.CheckPaymentDate : null)
                };
            }).ToList();

            receipt.TotalOriginal = totalOriginal;
            receipt.TotalBase = totalBase;

            await _uow.BeginTransactionAsync(ct);
            try
            {
                // 1) Crear recibo
                await _uow.Receipts.AddAsync(receipt);
                await _uow.SaveChangesAsync(ct);

                // 2) Crear espejo en Documents (Kind = Receipt)
                var mirror = new LedgerDocument
                {
                    PartyType = receipt.PartyType,
                    PartyId = receipt.PartyId,
                    Kind = LedgerDocumentKind.Receipt,
                    Status = LedgerDocumentStatus.Active,

                    SourceKind = SourceKind.AccountingReceipt,
                    SourceDocumentId = receipt.Id, // anclaje genérico
                    ReceiptId = receipt.Id,        // FK local

                    DisplayNumber = receipt.Number,
                    DocumentDate = receipt.Date,
                    Currency = receipt.Currency,
                    FxRate = receipt.FxRate,
                    TotalOriginal = receipt.TotalOriginal,
                    TotalBase = receipt.TotalBase
                };

                await _uow.LedgerDocuments.AddAsync(mirror);
                await _uow.SaveChangesAsync(ct);

                // 3) (Opcional) imputaciones iniciales
                if (req.Allocations is not null && req.Allocations.Count > 0)
                {
                    var debitIds = req.Allocations.Select(a => a.DebitDocumentId).Distinct().ToList();

                    var docs = await _uow.LedgerDocuments.Query()
                        .Where(d => debitIds.Contains(d.Id))
                        .Select(d => new { d.Id, d.PartyType, d.PartyId, d.Kind, d.Status, d.TotalBase })
                        .ToListAsync(ct);

                    var docsById = docs.ToDictionary(d => d.Id);

                    foreach (var a in req.Allocations)
                    {
                        if (!docsById.TryGetValue(a.DebitDocumentId, out var doc))
                            throw new InvalidOperationException($"Document {a.DebitDocumentId} not found.");

                        if (doc.Status != LedgerDocumentStatus.Active)
                            throw new InvalidOperationException($"Document {a.DebitDocumentId} is not active.");

                        if (doc.PartyType != receipt.PartyType || doc.PartyId != receipt.PartyId)
                            throw new InvalidOperationException($"Document {a.DebitDocumentId} belongs to another party.");

                        if (doc.Kind == LedgerDocumentKind.CreditNote)
                            throw new InvalidOperationException("Credit notes are not allocated via receipt.");
                    }

                    // b) Validar pendientes actuales por documento (usa repo de allocations, sin Query())
                    var appliedByDoc = await _uow.Allocations.GetAppliedByDocumentsAsync(debitIds, ct);

                    foreach (var a in req.Allocations)
                    {
                        var doc = docsById[a.DebitDocumentId];
                        appliedByDoc.TryGetValue(a.DebitDocumentId, out var already); // already: decimal
                        var pending = doc.TotalBase - already;

                        if (a.AmountBase > pending + 0.0001m)
                            throw new InvalidOperationException($"Allocation exceeds pending for document {a.DebitDocumentId}. Pending: {pending:N2}");
                    }

                    // c) Validar suma vs total del recibo
                    var sumAlloc = req.Allocations.Sum(x => x.AmountBase);
                    if (sumAlloc > receipt.TotalBase + 0.0001m)
                        throw new InvalidOperationException("Allocations exceed receipt total.");

                    // d) Crear allocations
                    foreach (var a in req.Allocations)
                    {
                        await _uow.Allocations.AddAsync(new Allocation
                        {
                            ReceiptId = receipt.Id,
                            DebitDocumentId = a.DebitDocumentId,
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

        public async Task<ReceiptDTO?> VoidReceiptAsync(int receiptId, CancellationToken ct = default)
        {
            var receipt = await _uow.Receipts.Query().FirstOrDefaultAsync(r => r.Id == receiptId, ct);
            if (receipt is null) return null;
            if (receipt.IsVoided) return Map(receipt);

            // ¿Tiene imputaciones? (usar helper del repo)
            var hasAlloc = (await _uow.Allocations.GetAppliedByReceiptsAsync(new[] { receiptId }, ct))
                           .ContainsKey(receiptId);
            if (hasAlloc)
                throw new InvalidOperationException("Cannot void a receipt with allocations. Deallocate first.");

            await _uow.BeginTransactionAsync(ct);
            try
            {
                receipt.IsVoided = true;
                receipt.VoidedAt = DateTime.UtcNow;

                var mirror = await _uow.LedgerDocuments.Query()
                    .FirstOrDefaultAsync(d => d.ReceiptId == receipt.Id, ct);

                if (mirror is not null && mirror.Status != LedgerDocumentStatus.Voided)
                {
                    mirror.Status = LedgerDocumentStatus.Voided;
                    mirror.VoidedAt = DateTime.UtcNow;
                }

                await _uow.SaveChangesAsync(ct);
                await _uow.CommitTransactionAsync(ct);
            }
            catch
            {
                await _uow.RollbackTransactionAsync(ct);
                throw;
            }

            return Map(receipt);
        }

        // ===== Asignación independiente (fuera de la creación del recibo) =====

        public async Task AllocateAsync(ReceiptAllocationCreateDTO req, CancellationToken ct = default)
        {
            if (req.AmountBase <= 0m)
                throw new InvalidOperationException("Amount must be > 0.");

            // 1) Recibo (no anulado)
            var receipt = await _uow.Receipts.Query()
                .FirstOrDefaultAsync(r => r.Id == req.ReceiptId, ct);

            if (receipt is null)
                throw new InvalidOperationException($"Receipt {req.ReceiptId} not found.");

            if (receipt.IsVoided)
                throw new InvalidOperationException("Cannot allocate from a voided receipt.");

            // 2) Documento débito (activo, mismo party, tipo válido)
            var debit = await _uow.LedgerDocuments.Query()
                .FirstOrDefaultAsync(d => d.Id == req.DebitDocumentId, ct);

            if (debit is null)
                throw new InvalidOperationException($"Debit document {req.DebitDocumentId} not found.");

            if (debit.Status != LedgerDocumentStatus.Active)
                throw new InvalidOperationException("Target debit document is not active.");

            if (debit.PartyType != receipt.PartyType || debit.PartyId != receipt.PartyId)
                throw new InvalidOperationException("Receipt and document belong to different parties.");

            if (debit.Kind != LedgerDocumentKind.Invoice && debit.Kind != LedgerDocumentKind.DebitNote)
                throw new InvalidOperationException("Only Invoice or Debit Note can receive allocations.");

            // 3) Disponible del recibo (no aplicado)
            var appliedFromReceiptDict = await _uow.Allocations.GetAppliedByReceiptsAsync(new[] { receipt.Id }, ct);
            var appliedFromReceipt = appliedFromReceiptDict.TryGetValue(receipt.Id, out var sumR) ? sumR : 0m;

            var receiptUnapplied = receipt.TotalBase - appliedFromReceipt;
            if (req.AmountBase > receiptUnapplied + 0.0001m)
                throw new InvalidOperationException(
                    $"Allocation exceeds receipt available. Available: {receiptUnapplied:N2} ARS");

            // 4) Pendiente del documento
            var appliedToDebitDict = await _uow.Allocations.GetAppliedByDocumentsAsync(new[] { debit.Id }, ct);
            var appliedToDebit = appliedToDebitDict.TryGetValue(debit.Id, out var sumD) ? sumD : 0m;

            var debitPending = debit.TotalBase - appliedToDebit;
            if (req.AmountBase > debitPending + 0.0001m)
                throw new InvalidOperationException(
                    $"Allocation exceeds debit pending. Pending: {debitPending:N2} ARS");

            // 5) Crear allocation
            await _uow.Allocations.AddAsync(new Allocation
            {
                ReceiptId = receipt.Id,
                DebitDocumentId = debit.Id,
                AmountBase = Math.Round(req.AmountBase, 2, MidpointRounding.ToEven),
                CreatedAt = DateTime.UtcNow
            });

            await _uow.SaveChangesAsync(ct);
        }

        public async Task DeallocateAsync(int allocationId, CancellationToken ct = default)
        {
            // Traigo la allocation para validar política antes de borrar
            var alloc = await _uow.Allocations.GetByIdAsync(allocationId);
            if (alloc is null) return;

            var receipt = await _uow.Receipts.Query()
                .FirstOrDefaultAsync(r => r.Id == alloc.ReceiptId, ct);

            if (receipt is not null && receipt.IsVoided)
                throw new InvalidOperationException("Cannot deallocate from a voided receipt.");

            // ✅ Usar el repo genérico
            await _uow.Allocations.DeleteAsync(allocationId);
            await _uow.SaveChangesAsync(ct);
        }


        private static ReceiptDTO Map(Receipt r) => new()
        {
            // ...
            Payments = r.PaymentLines.Select(p => new PaymentLineDTO
            {
                Id = p.Id,
                Method = p.Method,
                AmountOriginal = p.AmountOriginal,
                AmountBase = p.AmountBase,
                BankAccountId = p.BankAccountId,
                TransactionReference = p.TransactionReference,

                // Cheque
                CheckIssuerBankCode = p.CheckIssuerBankCode,
                CheckNumber = p.CheckNumber,
                CheckIssueDate = p.CheckIssueDate,
                CheckPaymentDate = p.CheckPaymentDate,
                CheckIssuerTaxId = p.CheckIssuerTaxId,
                CheckIssuerName = p.CheckIssuerName,
                CheckIsThirdParty = p.CheckIsThirdParty,

                Notes = p.Notes,
                ValueDate = p.ValueDate
            }).ToList()
        };

    }
}
