namespace FiscalDocumentationService.Business.Models
{
    public class FiscalDocumentCreateDTO
    {

        public int InvoiceType { get; set; }

        public int BuyerDocumentType { get; set; }
        public long BuyerDocumentNumber { get; set; }
        public int ReceiverVatConditionId { get; set; }

        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal ExemptAmount { get; set; } = 0;
        public decimal NonTaxableAmount { get; set; } = 0;
        public decimal OtherTaxesAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; }

        public int SalesOrderId { get; set; }

        public List<FiscalDocumentItemDTO> Items { get; set; } = new();

        public string Currency { get; set; } = "PES"; // Default currency
        public decimal ExchangeRate { get; set; } = 1; // Default exchange rate

        // Referencia a factura original (requerido para notas de débito y crédito)
        public int? ReferencedInvoiceType { get; set; } // Tipo de comprobante referenciado (ej: 1, 6)
        public int? ReferencedPointOfSale { get; set; } // Punto de venta del comprobante referenciado
        public long? ReferencedInvoiceNumber { get; set; } // Número del comprobante referenciado
    }
    public class FiscalDocumentItemDTO
    {
        public string Sku { get; set; } = string.Empty; // Product or service code
        public string Description { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int VatId { get; set; } = 5;
        public decimal VatBase { get; set; }
        public decimal VatAmount { get; set; }
        public string? DispatchCode { get; set; } // Optional dispatch code
        public int Warranty { get; set; } = 0; // Default warranty period in months 

    }




}

