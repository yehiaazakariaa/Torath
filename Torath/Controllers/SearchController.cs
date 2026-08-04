using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Torath.DTOs;
using Torath.Services;

namespace Torath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IElasticSearchService _elasticSearchService;

        public SearchController(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        // GET: api/search
        // We use [FromQuery] so Postman can send all the filters in the URL!
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] SearchRequestDto request)
        {
            var result = await _elasticSearchService.SearchAsync(request);
            return Ok(result);
        }
    }
}