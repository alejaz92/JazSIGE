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

        public async Task<CustomerStatementDTO> GetStatementAsync(
        PartyType partyType, int partyId,
        DateTime? from, DateTime? to,
        LedgerDocumentKind? kind, DocumentStatus? status,
        CancellationToken ct = default)
        {
            // 1) Base query (sin paginado), mismos filtros que ledger
            var q = _uow.LedgerDocuments.Query()
                .AsNoTracking()
                .Where(x => x.PartyType == partyType && x.PartyId == partyId);

            if (kind.HasValue)
                q = q.Where(x => x.Kind == kind.Value);

            if (status.HasValue)
                q = q.Where(x => x.Status == status.Value);

            // 2) Saldo inicial: todo antes de 'from'
            decimal opening = 0m;
            if (from.HasValue)
            {
                var prev = await q
                    .Where(x => x.DocumentDate < from.Value)
                    .Select(x => new
                    {
                        Debit = (x.Kind == LedgerDocumentKind.Invoice || x.Kind == LedgerDocumentKind.DebitNote) ? x.AmountARS : 0m,
                        Credit = (x.Kind == LedgerDocumentKind.CreditNote || x.Kind == LedgerDocumentKind.Receipt) ? x.AmountARS : 0m
                    })
                    .ToListAsync(ct);

                opening = prev.Sum(p => p.Debit) - prev.Sum(p => p.Credit);
            }

            // 3) Rango del extracto (Hasta inclusivo)
            if (from.HasValue) q = q.Where(x => x.DocumentDate >= from.Value);
            if (to.HasValue) q = q.Where(x => x.DocumentDate < to.Value.AddDays(1));

            var rows = await q
                .OrderBy(x => x.DocumentDate).ThenBy(x => x.Id)
                .Select(x => new
                {
                    x.DocumentDate,
                    x.Kind,
                    x.ExternalRefNumber,
                    // descripción legible construida
                    Description = (
                        (x.Kind == LedgerDocumentKind.Invoice ? "Factura " :
                         x.Kind == LedgerDocumentKind.DebitNote ? "Nota de débito " :
                         x.Kind == LedgerDocumentKind.CreditNote ? "Nota de crédito " :
                         x.Kind == LedgerDocumentKind.Receipt ? "Recibo " : string.Empty)
                        + (x.ExternalRefNumber ?? string.Empty)
                    ),
                    Debit = (x.Kind == LedgerDocumentKind.Invoice || x.Kind == LedgerDocumentKind.DebitNote) ? x.AmountARS : 0m,
                    Credit = (x.Kind == LedgerDocumentKind.CreditNote || x.Kind == LedgerDocumentKind.Receipt) ? x.AmountARS : 0m
                })
                .ToListAsync(ct);


            // 4) Armar DTO (running balance y totales)
            var dto = new CustomerStatementDTO
            {
                OpeningBalanceARS = opening
            };

            decimal running = opening, totalDebit = 0m, totalCredit = 0m;

            foreach (var r in rows)
            {
                running += r.Debit - r.Credit;
                totalDebit += r.Debit;
                totalCredit += r.Credit;

                dto.Items.Add(new CustomerStatementItemDTO
                {
                    DocumentDate = r.DocumentDate,
                    Kind = r.Kind switch
                    {
                        LedgerDocumentKind.Invoice => "invoice",
                        LedgerDocumentKind.DebitNote => "debitNote",
                        LedgerDocumentKind.CreditNote => "creditNote",
                        LedgerDocumentKind.Receipt => "receipt",
                        _ => r.Kind.ToString().ToLower()
                    },
                    ExternalRefNumber = r.ExternalRefNumber,
                    Description = r.Description,
                    DebitARS = r.Debit,
                    CreditARS = r.Credit,
                    RunningBalanceARS = running
                });
            }

            dto.Totals = new CustomerStatementTotalsDTO
            {
                DebitARS = totalDebit,
                CreditARS = totalCredit,
                ClosingBalanceARS = running
            };

            return dto;
        }
    }
}
