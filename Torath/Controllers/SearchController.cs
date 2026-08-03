using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Torath.Services;

namespace Torath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IElasticSearchService _elasticService;

        public SearchController(IElasticSearchService elasticService)
        {
            _elasticService = elasticService;
        }

        // GET: api/search?query=egypt
        [HttpGet]
        public async Task<IActionResult> GlobalSearch([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { message = "Search query cannot be empty." });
            }

            // Call your shiny new Elasticsearch method!
            var results = await _elasticService.SearchAsync(query);

            return Ok(new
            {
                searchQuery = query,
                totalResults = results.Count(), // Shows how many items matched
                data = results // Returns the mix of Books, Articles, Magazines, etc!
            });
        }
    }
}