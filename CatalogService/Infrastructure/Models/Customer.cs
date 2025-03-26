namespace CatalogService.Infrastructure.Models
{
    public class Customer : Actor
    {
        public string DeliveryAddress { get; set; }
        public int SellerId { get; set; }
        public int AssignedPriceListId { get; set; }
        public PriceList AssignedPriceList { get; set; }
    }
}
