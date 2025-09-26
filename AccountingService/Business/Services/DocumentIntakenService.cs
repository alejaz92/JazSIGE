using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Ledger;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Business.Services
{
    public class DocumentIntakeService : IDocumentIntakeService
    {
        private readonly IUnitOfWork _uow;

        public DocumentIntakeService(IUnitOfWork uow) => _uow = uow;

        public async Task<DocumentDTO> IngestFiscalAsync(FiscalDocumentCreateDTO req, CancellationToken ct = default)
        {
            if (req.PartyType != PartyType.Customer)
                throw new InvalidOperationException("Only Customer is allowed in V1.");

            // Idempotencia por (SourceKind, SourceDocumentId) — migramos desde tu Create por FiscalId. :contentReference[oaicite:7]{index=7}
            var existing = await _uow.LedgerDocuments.Query()
                .FirstOrDefaultAsync(d => d.SourceKind == req.SourceKind && d.SourceDocumentId == req.SourceDocumentId, ct);

            if (existing is not null)
                return Map(existing);

            // Validación mínima de consistencia Kind vs SourceKind (opcional)
            if (!IsKindCompatible(req.Kind, req.SourceKind))
                throw new InvalidOperationException($"Incompatible Kind {req.Kind} vs SourceKind {req.SourceKind}.");

            var entity = new LedgerDocument
            {
                PartyType = req.PartyType,
                PartyId = req.PartyId,
                Kind = req.Kind,
                Status = LedgerDocumentStatus.Active,

                SourceKind = req.SourceKind,
                SourceDocumentId = req.SourceDocumentId,
                DisplayNumber = req.DisplayNumber,

                DocumentDate = req.DocumentDate,
                Currency = req.Currency,
                FxRate = req.FxRate,
                TotalOriginal = req.TotalOriginal,
                TotalBase = Math.Round(req.TotalOriginal * req.FxRate, 2, MidpointRounding.ToEven)
            };

            await _uow.LedgerDocuments.AddAsync(entity);
            await _uow.SaveChangesAsync(ct);

            return Map(entity);
        }

        public async Task<DocumentDTO?> VoidFiscalAsync(SourceKind sourceKind, long sourceDocumentId, CancellationToken ct = default)
        {
            var doc = await _uow.LedgerDocuments.Query()
                .FirstOrDefaultAsync(d => d.SourceKind == sourceKind && d.SourceDocumentId == sourceDocumentId, ct);

            if (doc is null) return null;

            if (doc.Status == LedgerDocumentStatus.Voided) return Map(doc);

            doc.Status = LedgerDocumentStatus.Voided;
            doc.VoidedAt = DateTime.UtcNow;

            await _uow.SaveChangesAsync(ct);
            return Map(doc);
        }

        private static bool IsKindCompatible(LedgerDocumentKind kind, SourceKind source)
        {
            return (kind, source) switch
            {
                (LedgerDocumentKind.Invoice, SourceKind.FiscalInvoice) => true,
                (LedgerDocumentKind.DebitNote, SourceKind.FiscalDebitNote) => true,
                (LedgerDocumentKind.CreditNote, SourceKind.FiscalCreditNote) => true,
                _ => false
            };
        }

        private static DocumentDTO Map(LedgerDocument d) => new()
        {
            Id = d.Id,
            PartyType = d.PartyType,
            PartyId = d.PartyId,
            Kind = d.Kind,
            Status = d.Status,
            SourceKind = d.SourceKind,
            SourceDocumentId = d.SourceDocumentId,
            ReceiptId = d.ReceiptId,
            DocumentDate = d.DocumentDate,
            Currency = d.Currency,
            FxRate = d.FxRate,
            TotalOriginal = d.TotalOriginal,
            TotalBase = d.TotalBase,
            Number = d.DisplayNumber,
            CreatedAt = d.CreatedAt,
            VoidedAt = d.VoidedAt
        };
    }
}
