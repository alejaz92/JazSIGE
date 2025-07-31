namespace FiscalDocumentationService.Business.Models.Arca
{
    public class ArcaRequestDTO
    {
        public ArcaHeader header { get; set; } = new();
        public List<ArcaDetail> detail { get; set; } = new();
    }

    public class ArcaHeader
    {
        public int recordCount { get; set; }     // Usually 1
        public int pointOfSale { get; set; }     // Example: 1
        public int documentType { get; set; }    // Example: 01 = Invoice A, 06 = Invoice B
    }

    public class ArcaDetail
    {
        public int concept { get; set; } = 1;    // 1 = Goods, 2 = Services, 3 = Both
        public int buyerDocumentType { get; set; } // 80 = CUIT, 96 = DNI, 99 = Final Consumer
        public long buyerDocumentNumber { get; set; }

        public long invoiceFrom { get; set; }
        public long invoiceTo { get; set; }
        public string invoiceDate { get; set; } = string.Empty; // Format: YYYYMMDD

        public decimal totalAmount { get; set; }
        public decimal nonTaxableAmount { get; set; }
        public decimal netAmount { get; set; }
        public decimal exemptAmount { get; set; }
        public decimal vatAmount { get; set; }
        public decimal otherTaxesAmount { get; set; }

        public List<ArcaVAT> vat { get; set; } = new();
    }

    public class ArcaVAT
    {
        public int id { get; set; }              // 5 = 21%, 4 = 10.5%, etc.
        public decimal baseAmount { get; set; }
        public decimal amount { get; set; }
    }
}
