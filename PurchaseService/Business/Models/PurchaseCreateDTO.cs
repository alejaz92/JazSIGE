namespace PurchaseService.Business.Models
{
    public class PurchaseCreateDTO
    {
        public DateTime Date { get; set; }
        public int SupplierId { get; set; }
        public int WarehouseId { get; set; }
        public List<PurchaseArticleCreateDTO> Articles { get; set; } = new();
        public DispatchCreateDTO? Dispatch { get; set; } 
        public string reference { get; set; }
    }
}
