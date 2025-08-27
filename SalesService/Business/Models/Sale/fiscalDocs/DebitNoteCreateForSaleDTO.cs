namespace SalesService.Business.Models.Sale.fiscalDocs
{
    public class DebitNoteCreateForSaleDTO
    {
        public DebitNoteReason Reason { get; set; }    // p.ej. LatePaymentInterest, PriceUpCorrection, etc.

        public decimal NetAmount { get; set; }         // base imponible (obligatorio, > 0)
        public decimal? VatPercent { get; set; }       // 21 / 10.5, etc. (obligatorio si no se pasa VatAmount)
        public decimal? VatAmount { get; set; }        // si viene, prevalece sobre VatPercent

        public string? Comment { get; set; }           // texto libre
    }
}
