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
    }
}
