using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesService.Business.Interfaces;
using SalesService.Business.Models;
using System.Security.Claims;

namespace SalesService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ArticlePriceListController : ControllerBase
    {
        private readonly IArticlePriceListService _service;

        public ArticlePriceListController(IArticlePriceListService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene todos los precios actuales por lista de precios para un artículo
        /// </summary>
        [HttpGet("article/{articleId}")]
        public async Task<IActionResult> GetCurrentPricesByArticle(int articleId)
        {
            var prices = await _service.GetCurrentPricesByArticleAsync(articleId);
            return prices == null ? NotFound() : Ok(prices);
        }

        /// <summary>
        /// Obtiene el precio actual para un artículo en una lista de precios específica
        /// </summary>
        [HttpGet("article/{articleId}/pricelist/{priceListId}")]
        public async Task<IActionResult> GetCurrentPrice(int articleId, int priceListId)
        {
            var price = await _service.GetCurrentPriceAsync(articleId, priceListId);
            return price == null ? NotFound() : Ok(price);
        }

        /// <summary>
        /// Obtiene el historial completo de precios para un artículo en una lista de precios
        /// </summary>
        [HttpGet("history/article/{articleId}/pricelist/{priceListId}")]
        public async Task<IActionResult> GetPriceHistory(int articleId, int priceListId)
        {
            var prices = await _service.GetPriceHistoryAsync(articleId, priceListId);
            return prices == null ? NotFound() : Ok(prices);
        }

        /// <summary>
        /// Crea un nuevo precio para un artículo en una lista de precios
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ArticlePriceListCreateDTO dto)
        {
            if (!IsAdmin())
                return Forbid();

            try
            {
                var result = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetCurrentPrice), new { articleId = result.ArticleId, priceListId = result.PriceListId }, result);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }


        private bool IsAdmin()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            return roleClaim != null && roleClaim.Value == "Admin";
        }
    }
}
