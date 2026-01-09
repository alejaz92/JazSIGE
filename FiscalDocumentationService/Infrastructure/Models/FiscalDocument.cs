namespace FiscalDocumentationService.Infrastructure.Models
{
    public enum FiscalDocumentType { Invoice, CreditNote, DebitNote }
    public class FiscalDocument
    {
        public int Id { get; set; }

        public string DocumentNumber { get; set; } = string.Empty;
        public FiscalDocumentType Type { get; set; }         // Invoice, CreditNote, etc.
        public DateTime Date { get; set; }

        public string CAE { get; set; } = string.Empty;
        public DateTime CAEExpiration { get; set; }

        public int PointOfSale { get; set; } = 1;
        public int InvoiceType { get; set; }                 // Matches ARCA code (e.g., 1, 6)
        public long InvoiceFrom { get; set; }
        public long InvoiceTo { get; set; }

        public int BuyerDocumentType { get; set; }
        public long BuyerDocumentNumber { get; set; }
        public int ReceiverVatConditionId { get; set; } 

        public decimal NetAmount { get; set; }
        public decimal VATAmount { get; set; }
        public decimal ExemptAmount { get; set; }
        public decimal NonTaxableAmount { get; set; }
        public decimal OtherTaxesAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public int SalesOrderId { get; set; }

        public ICollection<FiscalDocumentItem> Items { get; set; } = new List<FiscalDocumentItem>();

        public string Currency { get; set; } = "PES"; // Default currency
        public decimal ExchangeRate { get; set; } = 1; // Default exchange rate
        public string IssuerTaxId { get; set; }



        // testing check
        public string EmissionProvider { get; set; } = "Dummy";
        public string ArcaEnvironment { get; set; } = "Homologation";


        // --- ARCA audit (minimum) ---
        public string ArcaStatus { get; set; } = "NotSent";
        public DateTime? ArcaLastInteractionAt { get; set; }

        // correlation / trace
        public Guid ArcaCorrelationId { get; set; } = Guid.NewGuid();

        // store provider payloads (safe: no secrets)
        public string? ArcaErrorsJson { get; set; } 
        public string? ArcaObservationsJson { get; set; }

        // Optional but very useful for debbuging
        public string? ArcaRequestJson { get; set; }
        public string? ArcaResponseJson { get; set; }
    }
}
