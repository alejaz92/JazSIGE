using AccountingService.Business.Interfaces;
using AccountingService.Business.Interfaces.Clients;
using AccountingService.Business.Models.Ledger;
using AccountingService.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Business.Services
{
    public class ReceiptQueryService : IReceiptQueryService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICatalogServiceClient _catalog;
        private readonly ICompanyServiceClient _company;

        public ReceiptQueryService(
            IUnitOfWork uow,
            ICatalogServiceClient catalog,
            ICompanyServiceClient company)
        {
            _uow = uow;
            _catalog = catalog;
            _company = company;
        }

        public async Task<ReceiptDetailDTO> GetDetailAsync(int receiptId, CancellationToken ct = default)
        {
            // 1) Recibo + pagos
            var receipt = await _uow.Receipts.Query()
                .Include(r => r.PaymentLines)
                .FirstOrDefaultAsync(r => r.Id == receiptId, ct)
                ?? throw new InvalidOperationException($"Receipt {receiptId} not found.");

            // 2) Espejo en LedgerDocuments (para DisplayNumber/fecha/moneda)
            var mirror = await _uow.LedgerDocuments.Query()
                .FirstOrDefaultAsync(d => d.ReceiptId == receipt.Id, ct);

            // 3) Datos de empresa
            var company = await _company.GetCompanyInfoAsync()
                ?? throw new InvalidOperationException("Company info not found.");

            // 4) Datos de cliente (v1: PartyType = Customer)
            var customer = await _catalog.GetCustomerByIdAsync(receipt.PartyId)
                ?? throw new InvalidOperationException($"Customer {receipt.PartyId} not found.");

            // 5) Imputaciones (allocations) + lookup de comprobantes débito
            var allocations = await _uow.Allocations.Query()
                .Where(a => a.ReceiptId == receipt.Id)
                .Select(a => new { a.DebitDocumentId, a.AmountBase })
                .ToListAsync(ct);

            var allocationDtos = new List<ReceiptAllocationDetailDTO>();
            if (allocations.Count > 0)
            {
                var debitIds = allocations.Select(a => a.DebitDocumentId).Distinct().ToList();
                var debitDocs = await _uow.LedgerDocuments.Query()
                    .Where(d => debitIds.Contains(d.Id))
                    .Select(d => new
                    {
                        d.Id,
                        d.Kind,
                        d.DisplayNumber,
                        d.DocumentDate
                        // Si agregás DueDate al modelo, proyectalo aquí y mapeá abajo.
                    })
                    .ToListAsync(ct);

                var byId = debitDocs.ToDictionary(d => d.Id);
                foreach (var a in allocations)
                {
                    if (!byId.TryGetValue(a.DebitDocumentId, out var d)) continue;

                    allocationDtos.Add(new ReceiptAllocationDetailDTO
                    {
                        DebitDocumentId = d.Id,
                        DebitDocumentKind = d.Kind.ToString(),
                        DebitDocumentNumber = d.DisplayNumber,
                        DebitDocumentDate = d.DocumentDate,
                        DebitDocumentDueDate = null, // completar si tenés DueDate
                        AppliedAmountBase = Math.Round(a.AmountBase, 2, MidpointRounding.ToEven)
                    });
                }
            }

            // 6) Pagos
            var paymentDtos = receipt.PaymentLines
                .OrderBy(p => p.Id)
                .Select(p => new ReceiptPaymentDTO
                {
                    Id = p.Id,
                    Method = p.Method.ToString(),
                    AmountOriginal = p.AmountOriginal,
                    AmountBase = p.AmountBase,
                    BankAccountId = p.BankAccountId,
                    TransactionReference = p.TransactionReference,
                    Notes = p.Notes,
                    ValueDate = p.ValueDate
                })
                .ToList();

            // 7) Armar DTO (nombres = tu contrato)
            var dto = new ReceiptDetailDTO
            {
                // Cabecera
                Id = receipt.Id,
                DocumentLetter = "X", // si guardás letra, mapeala aquí
                DocumentCode = mirror?.DisplayNumber ?? receipt.Number, // ej: "0001-00001234"
                DocumentDate = receipt.Date, // o mirror?.DocumentDate ?? receipt.Date

                // Company (coincide con tu DTO)
                CompanyName = company.Name,
                CompanyShortName = company.ShortName,
                CompanyTaxId = company.TaxId,
                CompanyAddress = company.Address,
                CompanyPostalCode = company.PostalCode,
                CompanyCity = company.City,
                CompanyProvince = company.Province,
                CompanyCountry = company.Country,
                CompanyPhone = company.Phone,
                CompanyEmail = company.Email,
                CompanyLogoUrl = company.LogoUrl,
                CompanyIVAType = company.IVAType,
                CompanyGrossIncome = company.GrossIncome,
                CompanyDateOfIncorporation = company.DateOfIncorporation,

                // Customer
                PartyId = receipt.PartyId,
                CustomerName = customer.CompanyName,
                CustomerIdType = "CUIT", // si querés inferir por formato, lo ajustamos luego
                CustomerTaxId = customer.TaxId,
                CustomerIVAType = customer.IVAType,
                CustomerSellCondition = customer.SellCondition,
                CustomerAddress = customer.Address,
                CustomerPostalCode = customer.PostalCode,
                CustomerCity = customer.City,
                CustomerProvince = customer.Province,
                CustomerCountry = customer.Country,

                // Moneda / Totales
                Currency = receipt.Currency,
                FxRate = receipt.FxRate,
                TotalOriginal = receipt.TotalOriginal,
                TotalBase = receipt.TotalBase,

                // Detalles
                Payments = paymentDtos,
                Allocations = allocationDtos,

                // Observaciones / Anulación
                Notes = receipt.Notes,
                IsVoided = receipt.IsVoided,
                VoidedAt = receipt.VoidedAt
            };

            return dto;
        }
    }
}
