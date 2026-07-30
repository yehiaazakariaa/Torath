using Microsoft.AspNetCore.Mvc;
using System.Threading;
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

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _newspaperService.GetAllAsync(page, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
        {
            var newspaper = await _newspaperService.GetByIdAsync(id, cancellationToken);
            if (newspaper == null) return NotFound();
            return Ok(newspaper);
        }

        [HttpGet("{id}/issues")]
        public async Task<IActionResult> GetIssues(int id, CancellationToken cancellationToken = default)
        {
            var issues = await _newspaperService.GetIssuesByNewspaperIdAsync(id, cancellationToken);
            return Ok(issues);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NewspaperWriteDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var createdNewspaper = await _newspaperService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdNewspaper.Id }, createdNewspaper);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] NewspaperWriteDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _newspaperService.UpdateAsync(id, request, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            await _newspaperService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}