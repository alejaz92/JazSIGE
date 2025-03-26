namespace CatalogService.Infrastructure.Models
{
    public class Actor : BaseEntity
    {
        public string TaxId { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Address { get; set; }
        public int PostalCodeId { get; set; }
        public PostalCode PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int IVATypeId { get; set; }
        public IVAType IVAType { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public int TransportId { get; set; }
        public Transport Transport { get; set; }
        public int SellConditionId { get; set; }
        public SellCondition SellCondition { get; set; }

    }
}
