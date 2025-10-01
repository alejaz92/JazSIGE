namespace AccountingService.Business.Models.Clients
{
    public class CustomerDTO
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;      // CUIT/CUIL/DNI
        public string IVAType { get; set; } = string.Empty;
        public string SellCondition { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        public string SellerName { get; set; } = string.Empty;


        public int? PostalCodeId { get; set; }
    }
}
