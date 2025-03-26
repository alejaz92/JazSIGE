﻿namespace CatalogService.Business.Models.Transport
{
    public class TransportDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? TaxId { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
    }
}
