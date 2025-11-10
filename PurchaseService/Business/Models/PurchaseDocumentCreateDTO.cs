using PurchaseService.Infrastructure.Models;
using System.ComponentModel.DataAnnotations;

namespace PurchaseService.Business.Models
{
    public class PurchaseDocumentCreateDTO
    {
        [Required]
        public PurchaseDocumentType Type { get; set; } // Invoice / DebitNote / CreditNote

        [Required, StringLength(64)]
        public string Number { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; }  // se espera UTC o con zona consistente

        // Solo USD o ARS por ahora
        [Required, RegularExpression("^(USD|ARS)$", ErrorMessage = "Currency must be USD or ARS.")]
        public string Currency { get; set; } = null!;

        // 1 si ARS, precisión alta
        [Range(0.000001, 999999999)]
        public decimal FxRate { get; set; }

        // Monto en la moneda original del documento
        [Range(0, 999999999999.99)]
        public decimal TotalAmount { get; set; }

        // URL al documento digitalizado (PDF/imagen)
        [Required, StringLength(512)]
        public string FileUrl { get; set; } = null!;
    }

    public class PurchaseDocumentCancelDTO
    {
        [Required, StringLength(256)]
        public string Reason { get; set; } = null!;
    }
}
