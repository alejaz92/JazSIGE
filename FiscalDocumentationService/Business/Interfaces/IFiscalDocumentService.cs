using FiscalDocumentationService.Business.Models;

namespace FiscalDocumentationService.Business.Interfaces
{
    /// <summary>
    /// Service interface for fiscal document management with ARCA/AFIP integration.
    /// Handles creation and retrieval of invoices, credit notes, and debit notes.
    /// </summary>
    public interface IFiscalDocumentService
    {
        /// <summary>
        /// Creates a new fiscal document and authorizes it with ARCA (if enabled).
        /// For invoices: implements idempotency - returns existing if SalesOrderId already has an invoice.
        /// For credit/debit notes: creates always (not idempotent).
        /// </summary>
        /// <param name="dto">Fiscal document creation request data</param>
        /// <returns>Created and authorized fiscal document with CAE</returns>
        /// <exception cref="FiscalValidationException">Thrown when validation fails (amounts, items, etc.)</exception>
        /// <exception cref="FiscalConfigurationException">Thrown when company fiscal settings are missing/invalid</exception>
        Task<FiscalDocumentDTO> CreateAsync(FiscalDocumentCreateDTO dto);

        /// <summary>
        /// Retrieves a fiscal document by its database ID.
        /// </summary>
        /// <param name="id">Fiscal document ID</param>
        /// <returns>Fiscal document with full details and items, or null if not found</returns>
        Task<FiscalDocumentDTO?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves the main invoice associated with a specific sales order.
        /// Useful for checking idempotent state and linking sales orders to fiscal documents.
        /// </summary>
        /// <param name="salesOrderId">Sales order ID</param>
        /// <returns>Associated invoice, or null if not found</returns>
        Task<FiscalDocumentDTO?> GetBySalesOrderIdAsync(int salesOrderId);

        /// <summary>
        /// Retrieves all credit notes (NC - Notas de Crédito) referencing a specific sale/invoice.
        /// Credit notes reduce the total amount of the original sale.
        /// </summary>
        /// <param name="saleId">ID of the original sale/invoice</param>
        /// <returns>List of credit notes for the sale (empty if none)</returns>
        Task<IReadOnlyList<FiscalDocumentDTO>> GetCreditNotesBySaleIdAsync(int saleId);

        /// <summary>
        /// Retrieves all debit notes (ND - Notas de Débito) referencing a specific sale/invoice.
        /// Debit notes increase the total amount of the original sale (e.g., additional charges).
        /// </summary>
        /// <param name="relatedId">ID of the original sale/invoice</param>
        /// <returns>List of debit notes for the sale (empty if none)</returns>
        Task<IReadOnlyList<FiscalDocumentDTO>> GetDebitNotesBySaleIdAsync(int relatedId);
    }
}
