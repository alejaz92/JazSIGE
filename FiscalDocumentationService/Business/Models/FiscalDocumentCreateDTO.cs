namespace FiscalDocumentationService.Business.Models
{
    /// <summary>
    /// Data Transfer Object for fiscal document creation request.
    /// Contains all required data to create a new fiscal document with ARCA.
    /// </summary>
    public class FiscalDocumentCreateDTO
    {
        /// <summary>
        /// ARCA invoice type code. 
        /// Examples: 1=Factura A (CUIT buyer), 6=Factura B (flexible buyer), 11=Factura C (Consumidor Final)
        ///          2,7,12,52=Debit Notes, 3,8,13,53=Credit Notes
        /// </summary>
        public int InvoiceType { get; set; }

        /// <summary>Buyer document type code (80=CUIT, 96=DNI, 99=Consumidor Final, etc.)</summary>
        public int BuyerDocumentType { get; set; }

        /// <summary>Buyer document number (CUIT, DNI number, etc.)</summary>
        public long BuyerDocumentNumber { get; set; }

        /// <summary>Buyer VAT condition ID (1=IVA Responsable, 2=IVA No Responsable, 3=Monotributo, 4=Exempt, 5=Consumidor Final, etc.)</summary>
        public int ReceiverVatConditionId { get; set; }

        /// <summary>Net taxable amount (subtotal before VAT)</summary>
        public decimal NetAmount { get; set; }

        /// <summary>VAT amount (21% typically, but can vary)</summary>
        public decimal VatAmount { get; set; }

        /// <summary>Tax-exempt amount (default 0 if not applicable)</summary>
        public decimal ExemptAmount { get; set; } = 0;

        /// <summary>Non-taxable amount like shipping, discounts (default 0 if not applicable)</summary>
        public decimal NonTaxableAmount { get; set; } = 0;

        /// <summary>Other taxes amount not included in VAT (default 0 if not applicable)</summary>
        public decimal OtherTaxesAmount { get; set; } = 0;

        /// <summary>Total invoice amount. Must equal: NetAmount + VatAmount + ExemptAmount + NonTaxableAmount + OtherTaxesAmount</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Related sales order ID (used for idempotency on invoices - returns existing if provided again)</summary>
        public int SalesOrderId { get; set; }

        /// <summary>Collection of line items for this document</summary>
        public List<FiscalDocumentItemDTO> Items { get; set; } = new();

        /// <summary>Currency code (default PES for Argentine Pesos)</summary>
        public string Currency { get; set; } = "PES";

        /// <summary>Exchange rate to reference currency (default 1 for PES)</summary>
        public decimal ExchangeRate { get; set; } = 1;

        /// <summary>
        /// Referenced invoice type (required for credit/debit notes).
        /// Type of the original document being referenced (e.g., 1 for Factura A, 6 for Factura B)
        /// </summary>
        public int? ReferencedInvoiceType { get; set; }

        /// <summary>
        /// Referenced point of sale (required for credit/debit notes).
        /// POS of the original document being referenced
        /// </summary>
        public int? ReferencedPointOfSale { get; set; }

        /// <summary>
        /// Referenced invoice number (required for credit/debit notes).
        /// Invoice number of the original document being referenced
        /// </summary>
        public long? ReferencedInvoiceNumber { get; set; }
    }

    /// <summary>
    /// Represents a single line item in a fiscal document.
    /// </summary>
    public class FiscalDocumentItemDTO
    {
        /// <summary>Product or service SKU code</summary>
        public string Sku { get; set; } = string.Empty;

        /// <summary>Item description (product name, service details, etc.)</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Unit price of the item</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>Quantity of items</summary>
        public int Quantity { get; set; }

        /// <summary>VAT rate ID (5=21%, 4=10.5%, 9=5%, 3=2.5%, 8=No Aplica, etc.)</summary>
        public int VatId { get; set; } = 5;

        /// <summary>VAT base amount (typically Quantity × UnitPrice)</summary>
        public decimal VatBase { get; set; }

        /// <summary>VAT amount calculated on base</summary>
        public decimal VatAmount { get; set; }

        /// <summary>Optional dispatch/tracking code (e.g., delivery tracking number)</summary>
        public string? DispatchCode { get; set; }

        /// <summary>Warranty period in months (0 if no warranty)</summary>
        public int Warranty { get; set; } = 0;
    }
}

