namespace FiscalDocumentationService.Business.Models
{
    /// <summary>
    /// Data Transfer Object for fiscal document response.
    /// Contains all details of a fiscal document (invoice, credit note, or debit note)
    /// including authorization status and AFIP QR URL for printing.
    /// </summary>
    public class FiscalDocumentDTO
    {
        /// <summary>Database identifier</summary>
        public int Id { get; set; }

        /// <summary>Formatted document number (e.g., "0001-00000001" = POS-Number)</summary>
        public string DocumentNumber { get; set; } = string.Empty;

        /// <summary>ARCA invoice type code (1=Factura A, 6=Factura B, 11=Factura C, 2=NC A, 3=ND A, etc.)</summary>
        public int InvoiceType { get; set; }

        /// <summary>Point of sale (POS) number assigned by ARCA</summary>
        public int PointOfSale { get; set; }

        /// <summary>Document issue date</summary>
        public DateTime Date { get; set; }

        /// <summary>CAE (Código de Autorización Electrónica) - electronic authorization code from ARCA</summary>
        public string Cae { get; set; } = string.Empty;

        /// <summary>CAE expiration date</summary>
        public DateTime CaeExpiration { get; set; }

        /// <summary>Buyer document type code (80=CUIT, 96=DNI, 99=Consumidor Final, etc.)</summary>
        public int BuyerDocumentType { get; set; }

        /// <summary>Buyer document number (CUIT, DNI, etc.)</summary>
        public long BuyerDocumentNumber { get; set; }

        /// <summary>Net taxable amount (subtotal before VAT)</summary>
        public decimal NetAmount { get; set; }

        /// <summary>VAT amount (IVA)</summary>
        public decimal VatAmount { get; set; }

        /// <summary>Tax-exempt amount</summary>
        public decimal ExemptAmount { get; set; }

        /// <summary>Non-taxable amount (e.g., shipping, discounts)</summary>
        public decimal NonTaxableAmount { get; set; }

        /// <summary>Other taxes amount (not VAT)</summary>
        public decimal OtherTaxesAmount { get; set; }

        /// <summary>Total amount = NetAmount + VatAmount + ExemptAmount + NonTaxableAmount + OtherTaxesAmount</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Related sales order ID (used for idempotency on invoices)</summary>
        public int SalesOrderId { get; set; }

        /// <summary>Invoice items (line items)</summary>
        public List<FiscalDocumentItemDTO> Items { get; set; } = new();

        /// <summary>Currency code (PES=Pesos argentinos, USD=Dólares, etc.)</summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>Exchange rate to USD (default 1 for PES)</summary>
        public decimal ExchangeRate { get; set; }

        /// <summary>Issuer CUIT (company tax ID)</summary>
        public string IssuerTaxId { get; set; } = string.Empty;

        /// <summary>Referenced invoice type - for credit/debit notes, the type of the original invoice</summary>
        public int? ReferencedInvoiceType { get; set; }

        /// <summary>Referenced point of sale - for credit/debit notes, the POS of the original invoice</summary>
        public int? ReferencedPointOfSale { get; set; }

        /// <summary>Referenced invoice number - for credit/debit notes, the number of the original invoice</summary>
        public long? ReferencedInvoiceNumber { get; set; }

        /// <summary>QR URL for AFIP/ARCA fiscal document (ready to embed in printed invoice)</summary>
        public string ArcaQrUrl { get; set; } = string.Empty;
    }
}
