namespace SalesService.Business.Models.Sale
{
    public class InvoiceDetailDTO
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public int InvoiceType { get; set; }
        public int PointOfSale { get; set; }
        public DateTime Date { get; set; }

        public string Cae { get; set; } = string.Empty;
        public DateTime CaeExpiration { get; set; }

        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }

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

        // customer
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerTaxID { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerPostalCode { get; set; } = string.Empty;
        public string CustomerCity { get; set; } = string.Empty;
        public string CustomerProvince { get; set; } = string.Empty;
        public string CustomerCountry { get; set; } = string.Empty;
        public string CustomerSellCondition { get; set; } = string.Empty;
        public string CustomerIVAType { get; set; } = string.Empty;

        // selller
        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;

        // List of items
        public List<InvoiceDetailItemDTO> Items { get; set; } = new();

        

    }

    public class InvoiceDetailItemDTO
    {
        public string Sku { get; set; } = string.Empty; // Product or service code
        public string Description { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int VatId { get; set; } = 5;
        public decimal VatPercentage { get; set; }
        public decimal VatBase { get; set; }
        public decimal VatAmount { get; set; }
        public string? DispatchCode { get; set; } // Optional dispatch code
        public int Warranty { get; set; } = 0; // Default warranty period in months
    }
}
