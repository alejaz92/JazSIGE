using FiscalDocumentationService.Infrastructure.Models;

/// <summary>
/// Entity representing a single line item in a fiscal document.
/// Items are always related to a parent FiscalDocument and are deleted when the document is deleted (cascade).
/// </summary>
public class FiscalDocumentItem
{
    /// <summary>Primary key</summary>
    public int Id { get; set; }

    /// <summary>Product or service SKU/code identifier</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Description of the product or service</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Unit price of the item</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Quantity of items being sold</summary>
    public int Quantity { get; set; }

    /// <summary>
    /// VAT rate ID (Aliquot ID from ARCA).
    /// Examples: 5=21%, 4=10.5%, 9=5%, 3=2.5%, 8=No Aplica
    /// </summary>
    public int VATId { get; set; }

    /// <summary>VAT base amount (taxable amount on which VAT is calculated)</summary>
    public decimal VATBase { get; set; }

    /// <summary>VAT amount calculated on the base</summary>
    public decimal VATAmount { get; set; }

    /// <summary>Foreign key - reference to parent FiscalDocument</summary>
    public int FiscalDocumentId { get; set; }

    /// <summary>Navigation property - parent FiscalDocument</summary>
    public FiscalDocument FiscalDocument { get; set; } = null!;

    /// <summary>Optional dispatch/tracking code (e.g., shipping tracking number)</summary>
    public string? DispatchCode { get; set; }

    /// <summary>Warranty period in months (0 if no warranty applies)</summary>
    public int Warranty { get; set; } = 0;
}
