using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Torath.DTOs;
using Torath.Services;

namespace Torath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User, Admin")] // Both roles can search the library
    public class SearchController : ControllerBase
    {
        private readonly IElasticSearchService _elasticSearchService;

        public SearchController(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] SearchRequestDto request)
        {
            var result = await _elasticSearchService.SearchAsync(request);
            return Ok(result);
        }
    }
}