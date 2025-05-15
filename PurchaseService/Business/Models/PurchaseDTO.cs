﻿namespace PurchaseService.Business.Models
{
    public class PurchaseDTO
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        
        public bool HasDispatch { get; set; }
        public bool StockUpdated { get; set; }

        public List<PurchaseArticleDTO> Articles { get; set; } = new();
        public DispatchDTO? Dispatch { get; set; } = null;

    }
}
