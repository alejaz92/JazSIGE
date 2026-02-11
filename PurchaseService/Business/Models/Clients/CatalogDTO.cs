namespace PurchaseService.Business.Models.Clients
{
    public class ArticleListDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }
        public int BrandId { get; set; }
        public string Brand { get; set; }
        public int LineId { get; set; }
        public string Line { get; set; }
        public int LineGroupId { get; set; }
        public string LineGroup { get; set; }
        public int UnitId { get; set; }
        public string Unit { get; set; }
        public bool IsTaxed { get; set; }
        public decimal IVAPercentage { get; set; }
        public int GrossIncomeTypeId { get; set; }
        public string GrossIncomeType { get; set; }
        public int Warranty { get; set; }
        public bool IsVisible { get; set; }
        public bool isActive { get; set; }
    }

    public class WarehouseListDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class SupplierListDTO
    {
        public int Id { get; set; }
        public string TaxId { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Address { get; set; }
        public int PostalCodeId { get; set; }
        public string PostalCode { get; set; }
        public int CityId { get; set; }
        public string City { get; set; }
        public int ProvinceId { get; set; }
        public string Province { get; set; }
        public int CountryId { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int IVATypeId { get; set; }
        public string IVAType { get; set; }
        public int IVATypeArcaCode { get; set; }
        public int WarehouseId { get; set; }
        public string Warehouse { get; set; }
        public int TransportId { get; set; }
        public string Transport { get; set; }
        public int SellConditionId { get; set; }
        public string SellCondition { get; set; }
        public bool IsActive { get; set; }
    }
}
