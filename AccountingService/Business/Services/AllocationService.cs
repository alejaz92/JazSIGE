// AccountingService.Business/Services/AllocationService.cs
namespace AccountingService.Business.Services
{
    using AccountingService.Business.Interfaces;
    using AccountingService.Business.Models.Receipts;
    using AccountingService.Infrastructure.Interfaces;
    using AccountingService.Infrastructure.Models;
    using Microsoft.EntityFrameworkCore;
    using static AccountingService.Infrastructure.Models.Enums;

    public class AllocationService : IAllocationService
    {
        private readonly IUnitOfWork _uow;
        public AllocationService(IUnitOfWork uow) => _uow = uow;

        public async Task CoverInvoiceWithReceiptsAsync(CoverInvoiceDTO dto, string? userName = null)
        {
            // Target (Invoice) por ExternalRefId & Kind
            var target = await _uow.LedgerDocuments.Query()
                .FirstOrDefaultAsync(x =>
                    x.PartyType == dto.PartyType &&
                    x.PartyId == dto.PartyId &&
                    x.Kind == LedgerDocumentKind.Invoice &&
                    x.ExternalRefId == dto.InvoiceExternalRefId &&
                    x.Status == DocumentStatus.Active);

            if (target == null) throw new InvalidOperationException("Target invoice not found.");
            if (target.PendingARS <= 0) throw new InvalidOperationException("Invoice already covered.");

            // Sources (recibos) válidos
            var srcIds = dto.Items.Select(i => i.SourceLedgerDocumentId).ToList();
            var sources = await _uow.LedgerDocuments.Query()
                .Where(x => srcIds.Contains(x.Id) &&
                            x.PartyType == dto.PartyType &&
                            x.PartyId == dto.PartyId &&
                            x.Kind == LedgerDocumentKind.Receipt &&
                            x.Status == DocumentStatus.Active &&
                            x.PendingARS > 0)
                .ToListAsync();

            if (sources.Count != srcIds.Count)
                throw new InvalidOperationException("Some source receipts are invalid or have no pending.");

            var sumApplied = dto.Items.Sum(i => i.AppliedARS);
            if (sumApplied != target.PendingARS)
                throw new InvalidOperationException("Sum of applied amounts must exactly match invoice pending.");

            // Auditoría de asignación cruzada
            var batch = new AllocationBatch
            {
                TargetDocumentId = target.Id,
                Reason = dto.Reason
            };
            await _uow.AllocationBatches.AddAsync(batch);

            foreach (var item in dto.Items)
            {
                var src = sources.First(s => s.Id == item.SourceLedgerDocumentId);
                if (item.AppliedARS <= 0 || item.AppliedARS > src.PendingARS)
                    throw new InvalidOperationException("Applied amount invalid for source receipt.");

                src.PendingARS -= item.AppliedARS;
                target.PendingARS -= item.AppliedARS;

                await _uow.AllocationItems.AddAsync(new AllocationItem
                {
                    AllocationBatch = batch,
                    SourceDocumentId = src.Id,
                    AppliedARS = item.AppliedARS
                });
            }

            if (target.PendingARS != 0)
                throw new InvalidOperationException("Invoice pending must be zero after allocation.");

            await _uow.SaveChangesAsync();
        }
    }
}
