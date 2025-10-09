using static AccountingService.Infrastructure.Models.Enums;

namespace AccountingService.Infrastructure.Models
{
    /// <summary>
    /// Renglón de cuenta corriente (Factura/ND/NC/Recibo).
    /// El vínculo con su origen es (Kind, ExternalRefId).
    /// - Docs fiscales: ExternalRefId = Id del FiscalDocumentationService
    /// - Recibos:      ExternalRefId = Id en la tabla local Receipt
    /// </summary>
    public class LedgerDocument : BaseEntity
    {
        // Generalización para clientes/proveedores
        public PartyType PartyType { get; set; }
        public int PartyId { get; set; } // CustomerId o SupplierId según PartyType

        public LedgerDocumentKind Kind { get; set; } // Invoice, DebitNote, CreditNote, Receipt

        /// <summary>
        /// Identificador externo INT del documento de origen.
        /// </summary>
        /// 
        public int ExternalRefId { get; set; } 
        public string ExternalRefNumber { get; set; } = string.Empty; // Número del doc. de origen (p.ej. "F0001-00001234")

        public DateTime DocumentDate { get; set; } // Fecha del documento de origen

        // Fiscales vendrán en ARS por regla de negocio, pero dejamos flexible para futuro
        public string Currency { get; set; } = "ARS";  // ISO 4217 p.ej. "ARS", "USD"
        public decimal FxRate { get; set; }            // 1 para ARS; sino cotización a ARS congelada al alta
        public decimal AmountOriginal { get; set; }
        public decimal AmountARS { get; set; }

        /// <summary>
        /// Saldo positivo: para débitos representa deuda; para créditos, crédito disponible.
        /// Nunca negativo.
        /// </summary>
        public decimal PendingARS { get; set; }

        public DocumentStatus Status { get; set; } = DocumentStatus.Active;

        // Importante: NO hay navegación a Receipt; el join se hace por (Kind=Receipt, ExternalRefId=Receipt.Id)
    }
}
