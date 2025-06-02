namespace SalesService.Business.Models.Clients
{
    public class TransportDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? TaxId { get; set; }
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
        public string Comment { get; set; }
        public bool IsActive { get; set; }
    }
}
