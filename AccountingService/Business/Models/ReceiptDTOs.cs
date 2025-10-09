namespace AccountingService.Business.Models.Receipts
{
    using static AccountingService.Infrastructure.Models.Enums;

    public class ReceiptPaymentCreateDTO
    {
        public PaymentMethod Method { get; set; }
        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; } = 1m;
        public decimal AmountOriginal { get; set; }
        public decimal AmountARS { get; set; }

        public int? BankAccountId { get; set; }
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }
        public DateTime? ValueDate { get; set; }

        public string? CheckBankCode { get; set; }
        public string? CheckNumber { get; set; }
        public string? CheckIssuer { get; set; }
        public bool? IsThirdPartyCheck { get; set; }
        public DateTime? CheckDueDate { get; set; }
    }

    public class ReceiptCreateDTO
    {
        public PartyType PartyType { get; set; }
        public int PartyId { get; set; }
        public DateTime DocumentDate { get; set; } = DateTime.UtcNow;
        public string? Number { get; set; }       // si lo generás luego, dejalo null
        public string? Notes { get; set; }

        // Seleccionados (TODOS deben quedar en 0 tras confirmar)
        public List<int> DebitDocumentIds { get; set; } = new();     // LedgerDocument.Id (Invoice + ND)
        public List<int> CreditDocumentIds { get; set; } = new();    // LedgerDocument.Id (NC)
        public List<int> ReceiptCreditIds { get; set; } = new();     // LedgerDocument.Id (Receipts con saldo)

        public List<ReceiptPaymentCreateDTO> Payments { get; set; } = new();
    }

    public class ReceiptDetailDTO
    {
        public int ReceiptId { get; set; }           // Receipt.Id
        public int LedgerId { get; set; }            // LedgerDocument.Id (Kind=Receipt, ExternalRefId=ReceiptId)
        public string? Number { get; set; }
        public string? LedgerExternalRefNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime DocumentDate { get; set; }

        public decimal TotalPaymentsARS { get; set; }
        public decimal RemainingInReceiptARS { get; set; } // saldo a favor que quedó

        public List<ReceiptPaymentDTO> Payments { get; set; } = new();
        public List<ReceiptAllocationDTO> Allocations { get; set; } = new();
    }

    public class ReceiptPaymentDTO
    {
        public int Id { get; set; }
        public PaymentMethod Method { get; set; }
        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; }
        public decimal AmountOriginal { get; set; }
        public decimal AmountARS { get; set; }
        public int? BankAccountId { get; set; }
        public string? TransactionReference { get; set; }
        public DateTime? ValueDate { get; set; }
        public string? Notes { get; set; }
        public string? CheckBankCode { get; set; }
        public string? CheckNumber { get; set; }
        public string? CheckIssuer { get; set; }
        public bool? IsThirdPartyCheck { get; set; }
        public DateTime? CheckDueDate { get; set; }
    }

    public class ReceiptAllocationDTO
    {
        public int Id { get; set; }
        public int TargetDocumentId { get; set; }   // LedgerDocument.Id
        public decimal AppliedARS { get; set; }
    }

    public class CoverInvoiceDTO
    {
        public PartyType PartyType { get; set; }
        public int PartyId { get; set; }

        // Target por FISCAL ExternalRefId para simplificar integración desde Sales
        public int InvoiceExternalRefId { get; set; }   // Kind=Invoice (o ND si quisieras reutilizar)
        public List<CoverInvoiceItemDTO> Items { get; set; } = new(); // Sources: recibos con saldo
        public string? Reason { get; set; }
    }

    public class CoverInvoiceItemDTO
    {
        public int SourceLedgerDocumentId { get; set; } // Kind=Receipt con saldo
        public decimal AppliedARS { get; set; }
    }

    public class ReceiptExportDTO
    {
        // ---------------------------
        // Documento
        // ---------------------------
        public int Id { get; set; }                     // Receipt.Id
        public string DocumentLetter { get; set; } = "X"; // por ahora fijo (futuro Fiscal)
        public string DocumentCode { get; set; } = "REC"; // identificador de tipo de doc
        public DateTime DocumentDate { get; set; }

        // ---------------------------
        // Empresa
        // ---------------------------
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyShortName { get; set; } = string.Empty;
        public string CompanyTaxId { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string CompanyPostalCode { get; set; } = string.Empty;
        public string CompanyCity { get; set; } = string.Empty;
        public string CompanyProvince { get; set; } = string.Empty;
        public string CompanyCountry { get; set; } = string.Empty;
        public string? CompanyPhone { get; set; }
        public string? CompanyEmail { get; set; }
        public string CompanyLogoUrl { get; set; } = string.Empty;
        public string CompanyIVAType { get; set; } = string.Empty;
        public string CompanyGrossIncome { get; set; } = string.Empty;
        public DateTime CompanyDateOfIncorporation { get; set; }

        // ---------------------------
        // Cliente
        // ---------------------------
        public int PartyId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerIdType { get; set; } = "CUIT";
        public string CustomerTaxId { get; set; } = string.Empty;
        public string CustomerIVAType { get; set; } = string.Empty;
        public string CustomerSellCondition { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerPostalCode { get; set; } = string.Empty;
        public string CustomerCity { get; set; } = string.Empty;
        public string CustomerProvince { get; set; } = string.Empty;
        public string CustomerCountry { get; set; } = string.Empty;

        // ---------------------------
        // Moneda / totales
        // ---------------------------
        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; } = 1m;
        public decimal TotalOriginal { get; set; }      // monto en moneda original
        public decimal TotalBase { get; set; }          // monto convertido a ARS

        // ---------------------------
        // Detalle
        // ---------------------------
        public List<ReceiptPaymentDTO> Payments { get; set; } = new();
        public List<ReceiptAllocationDetailDTO> Allocations { get; set; } = new();

        // ---------------------------
        // Misc
        // ---------------------------
        public string? Notes { get; set; }
        public bool IsVoided { get; set; } = false;
        public DateTime? VoidedAt { get; set; }
    }

    public class ReceiptAllocationDetailDTO
    {
        public int TargetDocumentId { get; set; }
        public string TargetDocumentNumber { get; set; } = string.Empty;
        public LedgerDocumentKind Kind { get; set; }
        public DateTime DocumentDate { get; set; }
        public decimal AppliedARS { get; set; }
    }
}
