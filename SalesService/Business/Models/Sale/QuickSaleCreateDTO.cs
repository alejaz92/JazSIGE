using SalesService.Business.Models.DeliveryNote;

namespace SalesService.Business.Models.Sale
{
    public class QuickSaleCreateDTO : SaleCreateDTO
    {
        // Depósito desde el que se egresa todo el stock
        public int WarehouseId { get; set; }
        // Observación opcional para el remito autogenerado
        public string? DeliveryNoteObservation { get; set; }
        // Observación opcional para la factura autogenerada (si la querés distinguir)
        public string? InvoiceObservation { get; set; }
    }

    public class QuickSaleResultDTO
    {
        public SaleDetailDTO Sale { get; set; } = default!;
        public DeliveryNoteDTO DeliveryNote { get; set; } = default!;
        public InvoiceBasicDTO Invoice { get; set; } = default!;
    }
}
