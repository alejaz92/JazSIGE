// AccountingService.Business/Services/ReceiptService.cs
namespace AccountingService.Business.Services
{
    using AccountingService.Business.Interfaces;
    using AccountingService.Business.Interfaces.Clients;
    using AccountingService.Business.Models.Receipts;
    using AccountingService.Infrastructure.Interfaces;
    using AccountingService.Infrastructure.Models;
    using JazSIGE.Accounting.Infrastructure.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using static AccountingService.Infrastructure.Models.Enums;

    public class ReceiptService : IReceiptService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICompanyServiceClient _companyClient;
        private readonly ICatalogServiceClient _catalogClient;

        public ReceiptService(IUnitOfWork uow, ICompanyServiceClient companyClient, ICatalogServiceClient catalogClient)
        {
            _uow = uow;
            _companyClient = companyClient;
            _catalogClient = catalogClient;
        }

        public async Task<ReceiptDetailDTO?> GetAsync(int receiptId)
        {
            var receipt = await _uow.Receipts.GetFullAsync(receiptId);
            if (receipt == null) return null;

            var ledger = await _uow.LedgerDocuments.Query()
                .FirstOrDefaultAsync(x => x.Kind == LedgerDocumentKind.Receipt && x.ExternalRefId == receipt.Id);

            return new ReceiptDetailDTO
            {
                ReceiptId = receipt.Id,
                LedgerId = ledger?.Id ?? 0,
                LedgerExternalRefNumber = ledger?.ExternalRefNumber, // NUEVO
                Number = receipt.Number,
                Notes = receipt.Notes,
                DocumentDate = ledger?.DocumentDate ?? DateTime.MinValue,
                TotalPaymentsARS = receipt.Payments.Sum(p => p.AmountARS),
                RemainingInReceiptARS = ledger?.PendingARS ?? 0m,
                Payments = receipt.Payments.Select(p => new ReceiptPaymentDTO
                {
                    Id = p.Id,
                    Method = p.Method,
                    Currency = p.Currency,
                    FxRate = p.FxRate,
                    AmountOriginal = p.AmountOriginal,
                    AmountARS = p.AmountARS,
                    BankAccountId = p.BankAccountId,
                    TransactionReference = p.TransactionReference,
                    ValueDate = p.ValueDate,
                    Notes = p.Notes,
                    CheckBankCode = p.CheckBankCode,
                    CheckNumber = p.CheckNumber,
                    CheckIssuer = p.CheckIssuer,
                    IsThirdPartyCheck = p.IsThirdPartyCheck,
                    CheckDueDate = p.CheckDueDate
                }).ToList(),
                Allocations = receipt.Allocations.Select(a => new ReceiptAllocationDTO
                {
                    Id = a.Id,
                    TargetDocumentId = a.TargetDocumentId,
                    AppliedARS = a.AppliedARS
                }).ToList()
            };
        }

        public async Task<ReceiptDetailDTO> CreateAsync(ReceiptCreateDTO dto, string? userName = null)
        {
            if (!dto.Payments.Any())
                throw new InvalidOperationException("Receipt must contain at least one payment.");

            var totalPayments = dto.Payments.Sum(p => p.AmountARS);

            var selectedIds = dto.DebitDocumentIds
                .Concat(dto.CreditDocumentIds)
                .Concat(dto.ReceiptCreditIds)
                .ToList();

            var targets = selectedIds.Any()
                ? await _uow.LedgerDocuments.Query()
                    .Where(x => selectedIds.Contains(x.Id) && x.Status == DocumentStatus.Active)
                    .ToListAsync()
                : new List<LedgerDocument>();

            if (targets.Any(t => t.PartyType != dto.PartyType || t.PartyId != dto.PartyId))
                throw new InvalidOperationException("All selected documents must belong to the same party.");

            var debits = targets.Where(t => t.Kind == LedgerDocumentKind.Invoice || t.Kind == LedgerDocumentKind.DebitNote).ToList();
            var credits = targets.Where(t => t.Kind == LedgerDocumentKind.CreditNote).ToList();
            var receiptCredits = targets.Where(t => t.Kind == LedgerDocumentKind.Receipt).ToList();

            var sumDebits = debits.Sum(t => t.PendingARS);
            var sumCredits = credits.Sum(t => t.PendingARS) + receiptCredits.Sum(t => t.PendingARS);
            var requiredFromPayments = Math.Max(0, sumDebits - sumCredits);

            if (totalPayments < requiredFromPayments)
                throw new InvalidOperationException("Total payments (ARS) must be >= debits minus credits.");

            if (targets.Any(d => d.PendingARS <= 0))
                throw new InvalidOperationException("All selected documents must have positive pending balance.");



            // Crear Receipt
            // 🔹 Generar número si no vino desde el front
            var receiptNumber = await GenerateNextReceiptNumberAsync();

            var receipt = new Receipt
            {
                Number = receiptNumber,
                Notes = dto.Notes
            };
            await _uow.Receipts.AddAsync(receipt);
            await _uow.SaveChangesAsync(); // obtener Id

            // Ledger del recibo
            var receiptLedger = new LedgerDocument
            {
                PartyType = dto.PartyType,
                PartyId = dto.PartyId,
                Kind = LedgerDocumentKind.Receipt,
                ExternalRefId = receipt.Id,
                ExternalRefNumber = receiptNumber,            
                DocumentDate = dto.DocumentDate,
                Currency = "ARS",
                FxRate = 1m,
                AmountOriginal = totalPayments,
                AmountARS = totalPayments,
                PendingARS = totalPayments,
                Status = DocumentStatus.Active
            };
            await _uow.LedgerDocuments.AddAsync(receiptLedger);

            // Payments
            foreach (var p in dto.Payments)
            {
                if (string.IsNullOrWhiteSpace(p.Currency)) p.Currency = "ARS";
                if (!string.Equals(p.Currency, "ARS", StringComparison.OrdinalIgnoreCase) && p.FxRate <= 0)
                    throw new InvalidOperationException("FxRate must be > 0 for non-ARS payments.");

                await _uow.ReceiptPayments.AddAsync(new ReceiptPayment
                {
                    Receipt = receipt,
                    Method = p.Method,
                    Currency = p.Currency,
                    FxRate = p.FxRate,
                    AmountOriginal = p.AmountOriginal,
                    AmountARS = p.AmountARS,
                    BankAccountId = p.BankAccountId,
                    TransactionReference = p.TransactionReference,
                    Notes = p.Notes,
                    ValueDate = p.ValueDate,
                    CheckBankCode = p.CheckBankCode,
                    CheckNumber = p.CheckNumber,
                    CheckIssuer = p.CheckIssuer,
                    IsThirdPartyCheck = p.IsThirdPartyCheck,
                    CheckDueDate = p.CheckDueDate
                });
            }

            // Helper interno (revisado)
            void ApplyAndAllocate(LedgerDocument target, decimal amountToApply, bool consumeReceipt)
            {
                if (amountToApply <= 0) return;

                target.PendingARS -= amountToApply;
                if (target.PendingARS < 0)
                    throw new InvalidOperationException("Target pending would become negative.");

                // Solo restamos del recibo si corresponde (débitos)
                if (consumeReceipt)
                {
                    receiptLedger.PendingARS -= amountToApply;
                    if (receiptLedger.PendingARS < 0)
                        throw new InvalidOperationException("Receipt pending would become negative.");
                }

                _uow.ReceiptAllocations.AddAsync(new ReceiptAllocation
                {
                    Receipt = receipt,
                    TargetDocumentId = target.Id,
                    AppliedARS = amountToApply
                }).GetAwaiter().GetResult();
            }

            // === Asignaciones ===
            // Créditos (NC y recibos previos) — se cierran, pero NO consumen el nuevo recibo
            foreach (var c in credits)
                ApplyAndAllocate(c, c.PendingARS, consumeReceipt: false);

            foreach (var r in receiptCredits)
                ApplyAndAllocate(r, r.PendingARS, consumeReceipt: false);

            // Débitos (facturas y ND) — sí consumen el recibo
            foreach (var d in debits)
                ApplyAndAllocate(d, d.PendingARS, consumeReceipt: true);


            var detail = await GetAsync(receipt.Id);
            return detail!;
        }

        public async Task<ReceiptExportDTO?> GetExportDataAsync(int receiptId)
        {
            // 1️⃣ Obtener entidades base
            var receipt = await _uow.Receipts.GetFullAsync(receiptId);
            if (receipt == null)
                return null;

            var ledger = await _uow.LedgerDocuments.Query()
                .FirstOrDefaultAsync(x => x.Kind == LedgerDocumentKind.Receipt && x.ExternalRefId == receipt.Id);
            if (ledger == null)
                return null;

            var company = await _companyClient.GetCompanyInfoAsync();
            var customer = await _catalogClient.GetCustomerByIdAsync(ledger.PartyId);
            if (company == null || customer == null)
                throw new InvalidOperationException("Company or Customer info not available.");

            // 2️⃣ Calcular totales
            var totalOriginal = receipt.Payments.Sum(p => p.AmountOriginal);
            var totalBase = receipt.Payments.Sum(p => p.AmountARS);

            // 3️⃣ Construir DTO de exportación
            var export = new ReceiptExportDTO
            {
                // Documento
                Id = receipt.Id,
                DocumentLetter = "X",
                DocumentCode = receipt.Number,
                DocumentDate = ledger.DocumentDate,

                // Empresa
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

                // Cliente
                PartyId = customer.Id,
                CustomerName = customer.CompanyName,
                CustomerIdType = "CUIT", // Por ahora fijo
                CustomerTaxId = customer.TaxId,
                CustomerIVAType = customer.IVAType,
                CustomerSellCondition = customer.SellCondition,
                CustomerAddress = customer.Address,
                CustomerPostalCode = customer.PostalCode,
                CustomerCity = customer.City,
                CustomerProvince = customer.Province,
                CustomerCountry = customer.Country,

                // Moneda / totales
                Currency = ledger.Currency,
                FxRate = ledger.FxRate,
                TotalOriginal = totalOriginal,
                TotalBase = totalBase,

                // Detalle
                Payments = receipt.Payments.Select(p => new ReceiptPaymentDTO
                {
                    Id = p.Id,
                    Method = p.Method,
                    Currency = p.Currency,
                    FxRate = p.FxRate,
                    AmountOriginal = p.AmountOriginal,
                    AmountARS = p.AmountARS,
                    BankAccountId = p.BankAccountId,
                    TransactionReference = p.TransactionReference,
                    ValueDate = p.ValueDate,
                    Notes = p.Notes,
                    CheckBankCode = p.CheckBankCode,
                    CheckNumber = p.CheckNumber,
                    CheckIssuer = p.CheckIssuer,
                    IsThirdPartyCheck = p.IsThirdPartyCheck,
                    CheckDueDate = p.CheckDueDate
                }).ToList(),

                Allocations = receipt.Allocations.Select(a => new ReceiptAllocationDetailDTO
                {
                    TargetDocumentId = a.TargetDocumentId,
                    Kind = _uow.LedgerDocuments.Query().FirstOrDefault(x => x.Id == a.TargetDocumentId)?.Kind ?? LedgerDocumentKind.Invoice,
                    DocumentDate = _uow.LedgerDocuments.Query().FirstOrDefault(x => x.Id == a.TargetDocumentId)?.DocumentDate ?? DateTime.MinValue,
                    TargetDocumentNumber = _uow.LedgerDocuments.Query().FirstOrDefault(x => x.Id == a.TargetDocumentId)?.ExternalRefNumber ?? string.Empty,
                    AppliedARS = a.AppliedARS
                }).ToList(),

                // Misc
                Notes = receipt.Notes,
                IsVoided = ledger.Status == DocumentStatus.Voided,
                VoidedAt = ledger.Status == DocumentStatus.Voided ? ledger.UpdatedAt : null
            };

            return export;
        }

        /// <summary>
        /// Genera el próximo número de recibo con formato 0001-00000001.
        /// POS por defecto = 0001 (podés reemplazarlo por un valor de Company/Config).
        /// Maneja concurrencia a nivel aplicación; idealmente acompañar con índice único en DB.
        /// </summary>
        private async Task<string> GenerateNextReceiptNumberAsync()
        {
            const int defaultPos = 1; // TODO: traer de configuración/empresa si corresponde
            string pos = defaultPos.ToString("0000");

            // Buscar el último ledger de tipo Recibo con ese POS y número formateado
            var last = await _uow.LedgerDocuments.Query()
                .Where(x => x.Kind == LedgerDocumentKind.Receipt
                            && x.ExternalRefNumber != null
                            && x.ExternalRefNumber.StartsWith(pos + "-"))
                .OrderByDescending(x => x.Id)
                .Select(x => x.ExternalRefNumber!)
                .FirstOrDefaultAsync();

            int next = 1;
            if (!string.IsNullOrWhiteSpace(last))
            {
                // last = "0001-00000042" -> extrae parte numérica
                var parts = last.Split('-', 2);
                if (parts.Length == 2 && int.TryParse(parts[1], out var current))
                    next = current + 1;
            }

            return $"{pos}-{next.ToString("00000000")}";
        }

    }
}
