using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Article;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : BaseController<Article, ArticleDTO, ArticleCreateDTO>
    {
        private readonly IArticleService _articleService;

        public ArticleController(IArticleService articleService) : base(articleService)
        {
            _articleService = articleService;
        }

        [HttpPut("{id}/toggle-visibility")]
        public async Task<IActionResult> UpdateVisibility(int id)
        {
            //var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            //if (userIdClaim == null)
            //    return Unauthorized();

            var updatedArticle = await _articleService.UpdateVisibilityAsync(id);
            if (updatedArticle == null)
                return NotFound();
            return Ok(updatedArticle);
        }

        [HttpGet("exists-by-description/{description}")]
        public async Task<IActionResult> ArticleExistsByDescription(string description)
        {
            var exists = await _articleService.ArticleExistsByDescription(description);
            return Ok(exists);
        }

        [HttpGet("exists-by-sku/{sku}")]
        public async Task<IActionResult> ArticleExistsBySKU(string sku)
        {
            var exists = await _articleService.ArticleExistsBySKU(sku);
            return Ok(exists);
        }


    }
}
