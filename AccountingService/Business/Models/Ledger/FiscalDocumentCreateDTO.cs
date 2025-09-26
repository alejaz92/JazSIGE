using AccountingService.Infrastructure.Models.Ledger;
using System.ComponentModel.DataAnnotations;

namespace AccountingService.Business.Models.Ledger
{
    // Intake request for a fiscal document (Invoice / Debit Note / Credit Note) into the Ledger.
    public class FiscalDocumentCreateDTO
    {
        [Required] public PartyType PartyType { get; set; } = PartyType.Customer;
        [Required] public int PartyId { get; set; }

        /// Invoice | DebitNote | CreditNote
        [Required] public LedgerDocumentKind Kind { get; set; }

        /// Must be one of the Fiscal* values
        [Required] public SourceKind SourceKind { get; set; }   // FiscalInvoice | FiscalDebitNote | FiscalCreditNote

        /// Id in the originating system (Fiscal service)
        [Required] public long SourceDocumentId { get; set; }

        [Required] public DateTime DocumentDate { get; set; }

        [Required, StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "ARS";

        [Range(0.000001, 999999)]
        public decimal FxRate { get; set; } = 1m;               // to ARS

        [Range(0.01, 999_999_999)]
        public decimal TotalOriginal { get; set; }              // in Currency

        /// Human readable number (e.g., "A 0001-00012345")
        [StringLength(50)]
        public string? DisplayNumber { get; set; }
    }
}
