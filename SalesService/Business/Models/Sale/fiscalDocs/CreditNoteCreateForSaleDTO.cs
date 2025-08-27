namespace SalesService.Business.Models.Sale.fiscalDocs
{
    public class CreditNoteCreateForSaleDTO
    {
        public CreditNoteReason Reason { get; set; }
        public List<CreditNoteItemDTO>? Items { get; set; } // Optional, depending on the reason
        public int? ReturnWarehouseId { get; set; } // Required if items are involved

        public decimal? NetAmount { get; set; } // Required for reasons that don't involve items
        public decimal? VatPercent { get; set; } // Required for reasons that don't involve items
        public decimal? VatAmount { get; set; } // Required for reasons that don't involve items

        public string? Comment { get; set; } // Optional comments


    }

    public class CreditNoteItemDTO
    {
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
    }
}
