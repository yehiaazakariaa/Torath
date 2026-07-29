using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Torath.DTOs;
using Torath.Services;

namespace Torath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewspapersController : ControllerBase
    {
        private readonly INewspaperService _newspaperService;

        public NewspapersController(INewspaperService newspaperService)
        {
            _newspaperService = newspaperService;
        }

        // 1. GET /api/newspapers?page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _newspaperService.GetAllAsync(page, pageSize);
            return Ok(result);
        }

        // 2. GET /api/newspapers/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var newspaper = await _newspaperService.GetByIdAsync(id);
            if (newspaper == null) return NotFound();
            return Ok(newspaper);
        }

        // 3. GET /api/newspapers/{id}/issues
        [HttpGet("{id}/issues")]
        public async Task<IActionResult> GetIssues(int id)
        {
            var issues = await _newspaperService.GetIssuesByNewspaperIdAsync(id);
            return Ok(issues);
        }

        // 4. POST /api/newspapers
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NewspaperWriteDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdNewspaper = await _newspaperService.CreateAsync(request);
            // Assuming your BaseContent has an 'Id' property
            return CreatedAtAction(nameof(GetById), new { id = createdNewspaper.Id }, createdNewspaper);
        }

        // 5. PUT /api/newspapers/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] NewspaperWriteDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _newspaperService.UpdateAsync(id, request);
            return NoContent(); // or Ok()
        }

        // 6. DELETE /api/newspapers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _newspaperService.DeleteAsync(id);
            return NoContent(); // or Ok()
        }
    }
}