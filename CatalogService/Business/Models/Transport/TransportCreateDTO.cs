namespace CatalogService.Business.Models.Transport
{
    public class TransportCreateDTO
    {
        public string Name { get; set; }
        public string? TaxId { get; set; }
        public string Address { get; set; }
        public int PostalCodeId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
    }
}
