using FiscalDocumentationService.Infrastructure.Models;

public class FiscalDocumentItem
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    public int VATId { get; set; }         // Ej: 5 = 21%
    public decimal VATBase { get; set; }   // Base imponible
    public decimal VATAmount { get; set; } // Monto del IVA

    public int FiscalDocumentId { get; set; }
    public FiscalDocument FiscalDocument { get; set; } = null!;

    public string? DispatchCode { get; set; } // Código de despacho opcional
}
