using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Torath.DTOs;
using Torath.Services;

namespace Torath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticlesController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        // GET /api/articles?page=1&pageSize=10&author=John
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? author = null)
        {
            // Passes the pagination and optional author filter to the service[cite: 1]
            var result = await _articleService.GetAllAsync(page, pageSize, author);
            return Ok(result);
        }

        // GET /api/articles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var article = await _articleService.GetByIdAsync(id);
            if (article == null) return NotFound();
            return Ok(article);
        }

        // POST /api/articles
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ArticleWriteDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdArticle = await _articleService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = createdArticle.Id }, createdArticle);
        }

        // PUT /api/articles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ArticleWriteDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _articleService.UpdateAsync(id, request);
            return NoContent();
        }

        // DELETE /api/articles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _articleService.DeleteAsync(id);
            return NoContent();
        }
    }
}