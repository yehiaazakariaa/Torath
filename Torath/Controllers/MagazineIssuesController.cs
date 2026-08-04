using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Torath.DTOs;
using Torath.Services;

namespace Torath.Controllers
{
    // Sets the base URL to /api/magazineissues
    [Route("api/[controller]")]
    [ApiController]
    public class MagazineIssuesController : ControllerBase
    {
        private readonly IMagazineIssueService _magazineIssueService;

        // Inject the service
        public MagazineIssuesController(IMagazineIssueService magazineIssueService)
        {
            _magazineIssueService = magazineIssueService;
        }

        // 1. GET /api/magazineissues?page=1&pageSize=10
        [HttpGet]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _magazineIssueService.GetAllAsync(page, pageSize);
            return Ok(result);
        }

        // 2. GET /api/magazineissues/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var issue = await _magazineIssueService.GetByIdAsync(id);
            if (issue == null) return NotFound();
            return Ok(issue);
        }

        // 3. GET /api/magazineissues/{id}/articles
        // This is the nested endpoint connecting issues to their granular content
        [HttpGet("{id}/articles")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetArticles(int id)
        {
            var articles = await _magazineIssueService.GetArticlesByIssueIdAsync(id);
            return Ok(articles);
        }

        // 4. POST /api/magazineissues
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] MagazineIssueWriteDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdIssue = await _magazineIssueService.CreateAsync(request);

            // Returns a 201 Created status and a link to the new resource
            return CreatedAtAction(nameof(GetById), new { id = createdIssue.Id }, createdIssue);
        }

        // 5. PUT /api/magazineissues/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] MagazineIssueWriteDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _magazineIssueService.UpdateAsync(id, request);
            return NoContent();
        }

        // 6. DELETE /api/magazineissues/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _magazineIssueService.DeleteAsync(id);
            return NoContent();
        }
    }
}