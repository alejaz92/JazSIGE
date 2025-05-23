﻿namespace SalesService.Business.Models.Article_PriceLists
{
    public class ArticlePriceListDTO
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public int PriceListId { get; set; }
        public string? PriceListName { get; set; }
        public decimal Price { get; set; }
        public DateTime EffectiveFrom { get; set; }
    }
}
