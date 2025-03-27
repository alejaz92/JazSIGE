﻿namespace CatalogService.Business.Models.Customer
{
    public class CustomerDTO
    {
        public int Id { get; set; }
        public string TaxId { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int IVATypeId { get; set; }
        public string IVAType { get; set; }
        public int WarehouseId { get; set; }
        public string Warehouse { get; set; }
        public int TransportId { get; set; }
        public string Transport { get; set; }
        public int SellConditionId { get; set; }
        public string SellCondition { get; set; }
        public string DeliverAddress { get; set; }
        public int SellerId { get; set; }
        public string SellerName { get; set; }
        public int AssignedPriceListId { get; set; }
        public string AssignedPriceList { get; set; }
    }
}
