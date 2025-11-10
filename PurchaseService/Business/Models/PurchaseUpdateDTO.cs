using System.ComponentModel.DataAnnotations;

namespace PurchaseService.Business.Models
{
    public class PurchaseArticlesUpdateItemDTO
    {
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }

    public class PurchaseArticlesUpdateDTO
    {
        [Required]
        public List<PurchaseArticlesUpdateItemDTO> Items { get; set; } = new();
    }
}
