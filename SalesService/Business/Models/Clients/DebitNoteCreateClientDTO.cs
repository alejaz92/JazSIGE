namespace SalesService.Business.Models.Clients
{
    public class DebitNoteCreateClientDTO
    {
        public int RelatedFiscalDocumentId { get; set; }
        public int BuyerDocumentType { get; set; }
        public long BuyerDocumentNumber { get; set; }

        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal ExemptAmount { get; set; } = 0;
        public decimal NonTaxableAmount { get; set; } = 0;
        public decimal OtherTaxesAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; }

        // Mantengo Items como opcional por compatibilidad (no usamos en ND por monto)
        public List<FiscalDocumentItemDTO> Items { get; set; } = new();

        public string Currency { get; set; } = "PES";
        public decimal ExchangeRate { get; set; } = 1;
        public string IssuerTaxId { get; set; } = string.Empty;

        public string? Reason { get; set; }
    }
}
