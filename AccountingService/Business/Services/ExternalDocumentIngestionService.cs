using AccountingService.Business.Interfaces;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models;
using static AccountingService.Infrastructure.Models.Enums;

namespace AccountingService.Business.Services
{
    public class ExternalDocumentIngestionService : IExternalDocumentIngestionService
    {
        private readonly IUnitOfWork _uow;

        public ExternalDocumentIngestionService(IUnitOfWork uow) => _uow = uow;

        public async Task<int> UpsertFiscalDocumentAsync(
            PartyType partyType, int partyId,
            LedgerDocumentKind kind, int externalRefId,string externalRefNumber , DateTime documentDate,
            decimal amountARS, string currency = "ARS", decimal fxRate = 1m)
        {
            if (kind == LedgerDocumentKind.Receipt)
                throw new InvalidOperationException("Receipts are local documents; do not ingest as external.");

            var existing = await _uow.LedgerDocuments.GetByExternalRefAsync(externalRefId, kind, partyType);
            if (existing != null)
            {
                // actualización mínima (si llegara a cambiar fecha/monto por corrección)
                existing.DocumentDate = documentDate;
                existing.AmountOriginal = amountARS; // docs fiscales vienen en ARS
                existing.AmountARS = amountARS;
                existing.Currency = currency;
                existing.FxRate = fxRate;
                existing.ExternalRefNumber = externalRefNumber;
                // Regla: pending = amount si está activo (si quisieras actualizar)
                if (existing.Status == DocumentStatus.Active)
                    existing.PendingARS = Math.Max(0, amountARS); // siempre positivo
                _uow.LedgerDocuments.Update(existing);
                await _uow.SaveChangesAsync();
                return existing.Id;
            }

            var doc = new LedgerDocument
            {
                PartyType = partyType,
                PartyId = partyId,
                Kind = kind,
                ExternalRefId = externalRefId,
                ExternalRefNumber = externalRefNumber,
                DocumentDate = documentDate,
                Currency = currency,
                FxRate = fxRate,
                AmountOriginal = amountARS,
                AmountARS = amountARS,
                PendingARS = Math.Max(0, amountARS),
                Status = DocumentStatus.Active
            };

            await _uow.LedgerDocuments.AddAsync(doc);
            await _uow.SaveChangesAsync();
            return doc.Id;
        }

        
        public async Task CancelFiscalDocumentAsync(
            PartyType partyType, int externalRefId, LedgerDocumentKind kind)
        {
            var existing = await _uow.LedgerDocuments.GetByExternalRefAsync(externalRefId, kind, partyType);
            if (existing == null)
                throw new InvalidOperationException("No se encontró el documento fiscal para cancelar.");
            existing.Status = DocumentStatus.Voided;
            existing.PendingARS = 0m; // al cancelar, no queda pendiente
            existing.UpdatedAt = DateTime.UtcNow;
            _uow.LedgerDocuments.Update(existing);
            await _uow.SaveChangesAsync();
        }
    }
}
