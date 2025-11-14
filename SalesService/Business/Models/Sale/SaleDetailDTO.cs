using SalesService.Business.Models.DeliveryNote;

namespace SalesService.Business.Models.Sale
{
    public class SaleDetailDTO
    {
        public int Id { get; set; }
        
        public DateTime Date {  get; set; }
        public decimal ExchangeRate { get; set; }
        public string? Observations { get; set; }
        public bool IsFinalConsumer { get; set; }
        public bool HasStockWarning { get; set; }



        // customer
        public string CustomerIdType { get; set; }
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


        // booleans
        public bool HasInvoice { get; set; }
        public bool HasDeliveryNotes { get; set; }
        public bool IsFullyDelivered { get; set; }
        

        //lists
        public List<SaleArticleDetailDTO> Articles { get; set; } = new();   
        
        public List<DeliveryNoteDTO> DeliveryNotes { get; set; } = new();
    }

    public class SaleArticleDetailDTO
    {
        public int ArticleId { get; set; }
        public string ArticleName { get; set; } = string.Empty;
        public string ArticleSKU {  get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal IVAPercent { get; set; }
        public decimal Subtotal => Math.Round((UnitPrice * Quantity) * (1 - DiscountPercent / 100), 2);
        public decimal IVAAmount => Math.Round(Subtotal * IVAPercent / 100, 2);
    }
}
