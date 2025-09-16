using AccountingService.Infrastructure.Models.Ledger;
using System.ComponentModel.DataAnnotations;

namespace AccountingService.Business.Models.Ledger
{
    public class DocumentRequestCreateDTO
    {
        [Required]
        public PartyType PartyType { get; set; } = PartyType.Customer; // V1: Customer

        [Required] 
        public int PartyId { get; set; }                                // V1: CustomerId

        [Required]
        public LedgerDocumentKind Kind { get; set; }                    // Invoice | DebitNote | CreditNote

        [Required] 
        public int FiscalDocumentId { get; set; }                       // Id del FiscalDocumentationService
        public string FiscalDocumentNumber { get; set; } = string.Empty; 

        [Required] 
        public DateTime DocumentDate { get; set; }

        [Required, StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "ARS";

        [Range(0.000001, 999999)]
        public decimal FxRate { get; set; } = 1m; // a ARS

        [Range(0.01, 999_999_999)]
        public decimal TotalOriginal { get; set; } // en Currency

        // Nota: TotalBase se calcula server-side = TotalOriginal * FxRate
    }
}
