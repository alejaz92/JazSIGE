namespace FiscalDocumentationService.Business.Models
{
    public class FiscalDocumentCreateDTO
    {
        public int PointOfSale { get; set; }
        public int InvoiceType { get; set; }

        public int BuyerDocumentType { get; set; }
        public long BuyerDocumentNumber { get; set; }

        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal ExemptAmount { get; set; } = 0;
        public decimal NonTaxableAmount { get; set; } = 0;
        public decimal OtherTaxesAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; }

        public int? SalesOrderId { get; set; }

        public List<FiscalDocumentItemDTO> Items { get; set; } = new();

        public string Currency { get; set; } = "PES"; // Default currency
        public decimal ExchangeRate { get; set; } = 1; // Default exchange rate
        public string IssuerTaxId { get; set; }
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




    // credit and debit notes 
    public class CreditNoteCreateDTO
    {
        // Debe apuntar SIEMPRE a la factura original
        public int RelatedFiscalDocumentId { get; set; }

        // Datos fiscales base: copiamos explícitos para no asumir nada en el servicio
        public int PointOfSale { get; set; }
        // Este InvoiceType DEBE ser el tipo de Nota correcto (03/08/13 según la factura base)
        public int InvoiceType { get; set; }

        public int BuyerDocumentType { get; set; }
        public long BuyerDocumentNumber { get; set; }

        // Totales (positivos). El signo conceptual lo determina el tipo de documento
        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal ExemptAmount { get; set; } = 0;
        public decimal NonTaxableAmount { get; set; } = 0;
        public decimal OtherTaxesAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; }

        public List<FiscalDocumentItemDTO> Items { get; set; } = new();

        public string Currency { get; set; } = "PES";
        public decimal ExchangeRate { get; set; } = 1;
        public string IssuerTaxId { get; set; }

        // Opcional: motivo de la nota (devolución, descuento, corrección)
        public string? Reason { get; set; }
    }

    public class DebitNoteCreateDTO
    {
        public int RelatedFiscalDocumentId { get; set; }

        public int PointOfSale { get; set; }
        // Este InvoiceType DEBE ser el tipo de Nota correcto (02/07/12 según la factura base)
        public int InvoiceType { get; set; }

        public int BuyerDocumentType { get; set; }
        public long BuyerDocumentNumber { get; set; }

        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal ExemptAmount { get; set; } = 0;
        public decimal NonTaxableAmount { get; set; } = 0;
        public decimal OtherTaxesAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; }

        public List<FiscalDocumentItemDTO> Items { get; set; } = new();

        public string Currency { get; set; } = "PES";
        public decimal ExchangeRate { get; set; } = 1;
        public string IssuerTaxId { get; set; }

        public string? Reason { get; set; }
    }
}

