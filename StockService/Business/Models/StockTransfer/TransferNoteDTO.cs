namespace StockService.Business.Models.StockTransfer
{
    public class TransferNoteDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public int OriginWarehouseId { get; set; }
        public string OriginWarehouseName { get; set; }
        public int DestinationWarehouseId { get; set; }
        public string DestinationWarehouseName { get; set; }


        public int NumberOfPackages { get; set; }
        public decimal DeclaredValue { get; set; }

        public string? Observations { get; set; }
        public int UserId { get; set; }

        public List<StockTransferArticleDTO> Articles { get; set; } = new();


        // Transport
        public int? TransportId { get; set; }
        public string? TransportName { get; set; } = string.Empty;
        public string? TransportAddress { get; set;} = string.Empty;
        public string? TransportPostalCode { get; set; } = string.Empty;
        public string? TransportCity { get; set; } = string.Empty;
        public string? TransportProvince { get; set; } = string.Empty;
        public string? TransportCountry { get; set; } = string.Empty;
        public string? TransportTaxId { get; set; } = string.Empty;


        // company
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyShortName { get; set; } = string.Empty;
        public string CompanyTaxId { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string CompanyPostalCode { get; set; } = string.Empty;
        public string CompanyCity { get; set; } = string.Empty;
        public string CompanyProvince { get; set; } = string.Empty;
        public string CompanyCountry { get; set; } = string.Empty;
        public string? CompanyPhone { get; set; }
        public string? CompanyEmail { get; set; } = string.Empty;
        public string CompanyLogoUrl { get; set; } = string.Empty;
        public string CompanyIVAType { get; set; }
        public string CompanyGrossIncome { get; set; } = string.Empty;
        public DateTime CompanyDateOfIncorporation { get; set; }
    }
}
