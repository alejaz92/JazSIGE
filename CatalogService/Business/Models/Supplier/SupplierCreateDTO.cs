namespace CatalogService.Business.Models.Supplier
{
    public class SupplierCreateDTO
    {
        public string TaxId { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Address { get; set; }
        public int PostalCodeId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int IVATypeId { get; set; }
        public int WarehouseId { get; set; }
        public int TransportId { get; set; }
        public int SellConditionId { get; set; }
    }
}
