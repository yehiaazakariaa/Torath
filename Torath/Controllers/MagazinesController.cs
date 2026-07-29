using Microsoft.AspNetCore.Mvc;
using Torath.DTOs;
using Torath.Services;

namespace Torath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MagazinesController : ControllerBase
    {
        private readonly IMagazineService _magazineService;

        public MagazinesController(IMagazineService magazineService)
        {
            _magazineService = magazineService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _magazineService.GetAllAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var magazine = await _magazineService.GetByIdAsync(id);
            if (magazine == null) return NotFound(new { message = "Magazine not found." });
            return Ok(magazine);
        }

        // --- NESTED ENDPOINT ---
        [HttpGet("{id}/issues")]
        public async Task<IActionResult> GetIssues(int id)
        {
            // First check if the parent magazine exists
            var magazine = await _magazineService.GetByIdAsync(id);
            if (magazine == null) return NotFound(new { message = "Magazine not found." });

            var issues = await _magazineService.GetIssuesByMagazineIdAsync(id);
            return Ok(issues);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MagazineWriteDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var magazine = await _magazineService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = magazine.Id }, magazine);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MagazineWriteDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var updated = await _magazineService.UpdateAsync(id, request);
            if (!updated) return NotFound(new { message = "Magazine not found." });
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _magazineService.DeleteAsync(id);
            if (!deleted) return NotFound(new { message = "Magazine not found." });
            return NoContent();
        }
    }
}