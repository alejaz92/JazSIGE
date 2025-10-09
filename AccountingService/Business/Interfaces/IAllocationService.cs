using AccountingService.Business.Models.Receipts;

namespace AccountingService.Business.Interfaces
{
    public interface IAllocationService
    {
        // Aplica recibos con saldo a una factura/ND (sin crear recibo ni pagos)
        Task CoverInvoiceWithReceiptsAsync(CoverInvoiceDTO dto, string? userName = null);
    }
}
