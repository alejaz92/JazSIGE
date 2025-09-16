using AccountingService.Business.Models.Ledger;
using AccountingService.Business.Services.Interfaces;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models.Ledger;

namespace AccountingService.Business.Services;

public class LedgerDocumentService : ILedgerDocumentService
{
    private readonly IUnitOfWork _uow;

    public LedgerDocumentService(IUnitOfWork uow) => _uow = uow;

    public async Task<DocumentResponseDTO> CreateAsync(DocumentRequestCreateDTO req, CancellationToken ct = default)
    {
        // V1: solo Customer
        if (req.PartyType != PartyType.Customer)
            throw new InvalidOperationException("Only Customer is allowed in V1.");

        // Idempotencia simple por FiscalDocumentId
        var existing = await _uow.LedgerDocuments.GetByFiscalIdAsync(req.FiscalDocumentId, ct);
        if (existing is not null)
            return Map(existing);

        var entity = new LedgerDocument
        {
            PartyType = req.PartyType,
            PartyId = req.PartyId,
            Kind = req.Kind,
            Status = LedgerDocumentStatus.Active,
            FiscalDocumentId = req.FiscalDocumentId,
            FiscalDocumentNumber = req.FiscalDocumentNumber, 
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

    private static DocumentResponseDTO Map(LedgerDocument d) => new()
    {
        Id = d.Id,
        PartyType = d.PartyType,
        PartyId = d.PartyId,
        Kind = d.Kind,
        Status = d.Status,
        FiscalDocumentId = d.FiscalDocumentId,
        DocumentDate = d.DocumentDate,
        Currency = d.Currency,
        FxRate = d.FxRate,
        TotalOriginal = d.TotalOriginal,
        TotalBase = d.TotalBase,
        CreatedAt = d.CreatedAt,
        VoidedAt = d.VoidedAt
    };
}
