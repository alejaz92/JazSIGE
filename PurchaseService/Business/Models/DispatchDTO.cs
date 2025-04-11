namespace PurchaseService.Business.Models
{
    public class DispatchDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int PurchaseId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; }

        public List<PurchaseArticleDTO> Articles { get; set; } = new();
    }
}
