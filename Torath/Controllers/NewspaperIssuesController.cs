using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Torath.DTOs;
using Torath.Services;

namespace Torath.Controllers
{
    [Route("api/[controller]")] // Maps URL to /api/newspaperissues
    [ApiController]             // Enables automatic model validation for [FromBody]
    public class NewspaperIssuesController : ControllerBase
    {
        private readonly INewspaperIssueService _newspaperIssueService;

        // Constructor: Injects the service interface
        public NewspaperIssuesController(INewspaperIssueService newspaperIssueService)
        {
            _newspaperIssueService = newspaperIssueService;
        }

        // 1. GET /api/newspaperissues?page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _newspaperIssueService.GetAllAsync(page, pageSize);
            return Ok(result); // Returns 200 OK with the pagination data
        }

        // 2. GET /api/newspaperissues/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var issue = await _newspaperIssueService.GetByIdAsync(id);
            if (issue == null) return NotFound(); // Returns 404 if ID doesn't exist
            return Ok(issue);                     // Returns 200 OK with the specific issue
        }

        // 3. GET /api/newspaperissues/{id}/articles
        [HttpGet("{id}/articles")]
        public async Task<IActionResult> GetArticles(int id)
        {
            var articles = await _newspaperIssueService.GetArticlesByIssueIdAsync(id);
            return Ok(articles); // Returns 200 OK with an array of articles
        }

        // 4. POST /api/newspaperissues
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NewspaperIssueWriteDto request)
        {
            // Check if the JSON payload is valid based on DTO rules
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdIssue = await _newspaperIssueService.CreateAsync(request);

            // Returns 201 Created and provides the URL to fetch the newly created resource
            return CreatedAtAction(nameof(GetById), new { id = createdIssue.Id }, createdIssue);
        }

        // 5. PUT /api/newspaperissues/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] NewspaperIssueWriteDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _newspaperIssueService.UpdateAsync(id, request);
            return NoContent(); // Returns 204 No Content to indicate a successful update
        }

        // 6. DELETE /api/newspaperissues/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _newspaperIssueService.DeleteAsync(id);
            return NoContent(); // Returns 204 No Content to indicate a successful deletion
        }
    }
}