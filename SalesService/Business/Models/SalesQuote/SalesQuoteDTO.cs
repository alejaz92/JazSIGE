public class SalesQuoteDTO
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public DateTime ExpirationDate { get; set; }

    // customer
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerTaxID { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string CustomerPostalCode { get; set; } = string.Empty;
    public string CustomerCity { get; set; } = string.Empty;
    public string CustomerProvince { get; set; } = string.Empty;
    public string CustomerCountry { get; set; } = string.Empty;

    // selller
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;

    // Transport
    public int TransportId { get; set; }
    public string TransportName { get; set; } = string.Empty;
    public string TransportPostalCode { get; set; } = string.Empty;
    public string TransportCity { get; set; } = string.Empty;
    public string TransportProvince { get; set; } = string.Empty;
    public string TransportCountry { get; set; } = string.Empty;

    // priceList
    public int PriceListId { get; set; }
    public string PriceListName { get; set; } = string.Empty;

    // company
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyTaxId { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyLogoUrl { get; set; } = string.Empty;

    // Calculations
    public decimal ExchangeRate { get; set; }
    public decimal SubtotalUSD { get; set; }
    public decimal IVAAmountUSD { get; set; }
    public decimal TotalUSD { get; set; }
    public string? Observations { get; set; }

    public List<SalesQuoteArticleDTO> Articles { get; set; } = new();
}
