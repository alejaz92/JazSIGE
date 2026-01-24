namespace FiscalDocumentationService.Infrastructure.Models
{
    /// <summary>
    /// Enumeration of fiscal document types
    /// </summary>
    public enum FiscalDocumentType { Invoice, CreditNote, DebitNote }

    /// <summary>
    /// Entity representing a fiscal document (Invoice, Credit Note, or Debit Note).
    /// This entity is persisted to database and includes audit fields for ARCA integration tracking.
    /// </summary>
    public class FiscalDocument
    {
        /// <summary>Primary key</summary>
        public int Id { get; set; }

        /// <summary>Formatted document number (POS-Number format, e.g., "0001-00000001")</summary>
        public string DocumentNumber { get; set; } = string.Empty;

        /// <summary>Document type enum (Invoice, CreditNote, DebitNote)</summary>
        public FiscalDocumentType Type { get; set; }

        /// <summary>Issue date of the document</summary>
        public DateTime Date { get; set; }

        /// <summary>CAE (Código de Autorización Electrónica) - authorization code from ARCA</summary>
        public string CAE { get; set; } = string.Empty;

        /// <summary>CAE expiration date - when the authorization becomes invalid</summary>
        public DateTime CAEExpiration { get; set; }

        /// <summary>Point of sale (POS) number assigned by ARCA</summary>
        public int PointOfSale { get; set; } = 1;

        /// <summary>ARCA invoice type code (1=Factura A, 6=Factura B, 11=Factura C, 2=NC A, 3=ND A, etc.)</summary>
        public int InvoiceType { get; set; }

        /// <summary>Starting invoice number in range (usually same as InvoiceTo for single documents)</summary>
        public long InvoiceFrom { get; set; }

        /// <summary>Ending invoice number in range (usually same as InvoiceFrom for single documents)</summary>
        public long InvoiceTo { get; set; }

        /// <summary>Buyer document type code (80=CUIT, 96=DNI, 99=Consumidor Final, etc.)</summary>
        public int BuyerDocumentType { get; set; }

        /// <summary>Buyer document number (CUIT, DNI, etc.)</summary>
        public long BuyerDocumentNumber { get; set; }

        /// <summary>Buyer VAT condition ID (1=Responsable, 2=No Responsable, 5=Consumidor Final, etc.)</summary>
        public int ReceiverVatConditionId { get; set; }

        /// <summary>Net taxable amount (subtotal before VAT)</summary>
        public decimal NetAmount { get; set; }

        /// <summary>VAT amount (IVA - typically 21%)</summary>
        public decimal VATAmount { get; set; }

        /// <summary>Tax-exempt amount</summary>
        public decimal ExemptAmount { get; set; }

        /// <summary>Non-taxable amount (e.g., shipping, discounts)</summary>
        public decimal NonTaxableAmount { get; set; }

        /// <summary>Other taxes amount (taxes other than VAT)</summary>
        public decimal OtherTaxesAmount { get; set; }

        /// <summary>Total document amount</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Related sales order ID (used for idempotency on invoices)</summary>
        public int SalesOrderId { get; set; }

        /// <summary>Collection of line items in this document</summary>
        public ICollection<FiscalDocumentItem> Items { get; set; } = new List<FiscalDocumentItem>();

        /// <summary>Currency code (PES=Argentine Pesos, USD=US Dollars, etc.)</summary>
        public string Currency { get; set; } = "PES";

        /// <summary>Exchange rate (default 1 for PES)</summary>
        public decimal ExchangeRate { get; set; } = 1;

        /// <summary>Issuer CUIT (company tax ID) - stored without formatting</summary>
        public string IssuerTaxId { get; set; } = string.Empty;

        /// <summary>Referenced invoice type (for credit/debit notes only) - type of original document</summary>
        public int? ReferencedInvoiceType { get; set; }

        /// <summary>Referenced point of sale (for credit/debit notes only) - POS of original document</summary>
        public int? ReferencedPointOfSale { get; set; }

        /// <summary>Referenced invoice number (for credit/debit notes only) - number of original document</summary>
        public long? ReferencedInvoiceNumber { get; set; }

        /// <summary>Emission provider identifier (e.g., "Dummy" for dev/test, "WSFE" for production ARCA)</summary>
        public string EmissionProvider { get; set; } = "Dummy";

        /// <summary>ARCA environment (Homologation or Production)</summary>
        public string ArcaEnvironment { get; set; } = "Homologation";

        /// <summary>Current status in ARCA flow (Pending, Authorized, Rejected, etc.)</summary>
        public string ArcaStatus { get; set; } = "NotSent";

        /// <summary>Timestamp of last interaction with ARCA</summary>
        public DateTime? ArcaLastInteractionAt { get; set; }

        /// <summary>Correlation ID for tracing interactions with ARCA (for debugging)</summary>
        public Guid ArcaCorrelationId { get; set; } = Guid.NewGuid();

        /// <summary>JSON-serialized ARCA errors (if any) - safe to store, no secrets</summary>
        public string? ArcaErrorsJson { get; set; }

        /// <summary>JSON-serialized ARCA observations/warnings - safe to store, no secrets</summary>
        public string? ArcaObservationsJson { get; set; }

        /// <summary>JSON-serialized ARCA request payload - useful for debugging and audit trails</summary>
        public string? ArcaRequestJson { get; set; }

        /// <summary>JSON-serialized ARCA response payload - useful for debugging and audit trails</summary>
        public string? ArcaResponseJson { get; set; }
    }
}
