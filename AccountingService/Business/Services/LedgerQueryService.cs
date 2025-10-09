using AccountingService.Business.Interfaces;
using AccountingService.Business.Models;
using AccountingService.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static AccountingService.Infrastructure.Models.Enums;

namespace AccountingService.Business.Services
{
    public class LedgerQueryService : ILedgerQueryService
    {
        private readonly IUnitOfWork _uow;
        public LedgerQueryService(IUnitOfWork uow) => _uow = uow;

        public async Task<BalancesDTO> GetBalancesAsync(PartyType partyType, int partyId)
        {
            var q = _uow.LedgerDocuments.Query()
                .Where(x => x.PartyType == partyType && x.PartyId == partyId && x.Status == DocumentStatus.Active);

            var pendingDebits = await q.Where(x => x.Kind == LedgerDocumentKind.Invoice || x.Kind == LedgerDocumentKind.DebitNote)
                                       .SumAsync(x => (decimal?)x.PendingARS) ?? 0m;

            var creditNotes = await q.Where(x => x.Kind == LedgerDocumentKind.CreditNote)
                                     .SumAsync(x => (decimal?)x.PendingARS) ?? 0m;

            var receiptsCredit = await q.Where(x => x.Kind == LedgerDocumentKind.Receipt)
                                        .SumAsync(x => (decimal?)x.PendingARS) ?? 0m;

            return new BalancesDTO
            {
                PendingToPayARS = Math.Max(0, pendingDebits - creditNotes),
                CreditInReceiptsARS = receiptsCredit
            };
        }

        public async Task<PagedResult<LedgerDocumentDTO>> GetLedgerAsync(
            PartyType partyType, int partyId, DateTime? from, DateTime? to,
            LedgerDocumentKind? kind, DocumentStatus? status, int page, int pageSize)
        {
            var q = _uow.LedgerDocuments.Query()
                .Where(x => x.PartyType == partyType && x.PartyId == partyId);

            if (from.HasValue) q = q.Where(x => x.DocumentDate >= from.Value);
            if (to.HasValue) q = q.Where(x => x.DocumentDate < to.Value.AddDays(1));
            if (kind.HasValue) q = q.Where(x => x.Kind == kind.Value);
            if (status.HasValue) q = q.Where(x => x.Status == status.Value);

            var total = await q.CountAsync();

            var items = await q.OrderByDescending(x => x.DocumentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LedgerDocumentDTO
                {
                    Id = x.Id,
                    PartyType = x.PartyType,
                    PartyId = x.PartyId,
                    Kind = x.Kind,
                    ExternalRefId = x.ExternalRefId,
                    ExternalRefNumber = x.ExternalRefNumber,
                    DocumentDate = x.DocumentDate,
                    Currency = x.Currency,
                    FxRate = x.FxRate,
                    AmountOriginal = x.AmountOriginal,
                    AmountARS = x.AmountARS,
                    PendingARS = x.PendingARS,
                    Status = x.Status
                }).ToListAsync();

            return new PagedResult<LedgerDocumentDTO>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }

        public async Task<SelectablesDTO> GetSelectablesForReceiptAsync(PartyType partyType, int partyId)
        {
            var debits = await _uow.LedgerDocuments.GetSelectablesDebitsAsync(partyType, partyId);
            var credits = await _uow.LedgerDocuments.GetSelectablesCreditsAsync(partyType, partyId);
            var receiptCredits = await _uow.LedgerDocuments.GetReceiptCreditsAsync(partyType, partyId);

            return new SelectablesDTO
            {
                Debits = debits.Select(x => new SimpleDocDTO
                {
                    Id = x.Id,
                    Kind = x.Kind,
                    ExternalRefId = x.ExternalRefId,
                    ExternalRefNumber = x.ExternalRefNumber,  
                    DocumentDate = x.DocumentDate,
                    PendingARS = x.PendingARS
                }).ToList(),
                Credits = credits.Select(x => new SimpleDocDTO
                {
                    Id = x.Id,
                    Kind = x.Kind,
                    ExternalRefId = x.ExternalRefId,
                    ExternalRefNumber = x.ExternalRefNumber,  
                    DocumentDate = x.DocumentDate,
                    PendingARS = x.PendingARS
                }).ToList(),
                ReceiptCredits = receiptCredits.Select(x => new SimpleDocDTO
                {
                    Id = x.Id,
                    Kind = x.Kind,
                    ExternalRefId = x.ExternalRefId,
                    ExternalRefNumber = x.ExternalRefNumber,  
                    DocumentDate = x.DocumentDate,
                    PendingARS = x.PendingARS
                }).ToList()
            };
        }

        public async Task<List<SimpleDocDTO>> GetReceiptCreditsAsync(PartyType partyType, int partyId)
        {
            var list = await _uow.LedgerDocuments.GetReceiptCreditsAsync(partyType, partyId);
            return list.Select(x => new SimpleDocDTO
            {
                Id = x.Id,
                Kind = x.Kind,
                ExternalRefId = x.ExternalRefId,
                ExternalRefNumber = x.ExternalRefNumber, // NUEVO
                DocumentDate = x.DocumentDate,
                PendingARS = x.PendingARS
            }).ToList();
        }
    }
}
