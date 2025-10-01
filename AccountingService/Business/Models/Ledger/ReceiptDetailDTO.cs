namespace AccountingService.Business.Models.Ledger
{
    public class ReceiptDetailDTO
    {
        // Encabezado documento reutilizable
        
        public int Id { get; set; }
        public string DocumentCode { get; set; } = string.Empty; // Ej: "X - 0001 00001234"
        public DateTime DocumentDate { get; set; }
        public string DocumentLetter { get; set; } = "X"; // Ej: "X"



        // company
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyShortName { get; set; } = string.Empty;
        public string CompanyTaxId { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string CompanyPostalCode { get; set; } = string.Empty;
        public string CompanyCity { get; set; } = string.Empty;
        public string CompanyProvince { get; set; } = string.Empty;
        public string CompanyCountry { get; set; } = string.Empty;
        public string? CompanyPhone { get; set; }
        public string? CompanyEmail { get; set; } = string.Empty;
        public string CompanyLogoUrl { get; set; } = string.Empty;
        public string CompanyIVAType { get; set; }
        public string CompanyGrossIncome { get; set; } = string.Empty;
        public DateTime CompanyDateOfIncorporation { get; set; }

        // Customer
        public int PartyId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerIdType { get; set; } = string.Empty; // CUIT/CUIL/DNI
        public string CustomerTaxId { get; set; } = string.Empty;
        public string CustomerIVAType { get; set; } = string.Empty;
        public string CustomerSellCondition { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerPostalCode { get; set; } = string.Empty;
        public string CustomerCity { get; set; } = string.Empty;
        public string CustomerProvince { get; set; } = string.Empty;
        public string CustomerCountry { get; set; } = string.Empty;

        // Moneda / totales
        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; }
        public decimal TotalOriginal { get; set; }
        public decimal TotalBase { get; set; }

        // Detalles
        public List<ReceiptPaymentDTO> Payments { get; set; } = new();
        public List<ReceiptAllocationDetailDTO> Allocations { get; set; } = new();

        // Observaciones
        public string? Notes { get; set; }
        public bool IsVoided { get; set; }
        public DateTime? VoidedAt { get; set; }
    }

    public class ReceiptPaymentDTO
    {
        public int Id { get; set; }
        public string Method { get; set; } = string.Empty;  // cash / bankTransfer / etc.
        public decimal AmountOriginal { get; set; }
        public decimal AmountBase { get; set; }
        public int? BankAccountId { get; set; }
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }
        public DateTime? ValueDate { get; set; }

        // Cheque
        public string? CheckIssuerBankCode { get; set; }
        public string? CheckNumber { get; set; }
        public DateTime? CheckIssueDate { get; set; }
        public DateTime? CheckPaymentDate { get; set; }
        public string? CheckIssuerTaxId { get; set; }
        public string? CheckIssuerName { get; set; }
        public bool? CheckIsThirdParty { get; set; }
    }

    public class ReceiptAllocationDetailDTO
    {
        public int DebitDocumentId { get; set; }
        public string DebitDocumentKind { get; set; } = string.Empty; // Invoice / DebitNote
        public string DebitDocumentNumber { get; set; } = string.Empty; // DisplayNumber
        public DateTime DebitDocumentDate { get; set; }
        public DateTime? DebitDocumentDueDate { get; set; } // si lo incorporás en LedgerDocuments
        public decimal AppliedAmountBase { get; set; }
    }
}
