using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
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
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _magazineService.GetAllAsync(page, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
        {
            var magazine = await _magazineService.GetByIdAsync(id, cancellationToken);
            if (magazine == null) return NotFound();
            return Ok(magazine);
        }

        [HttpGet("{id}/issues")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetIssues(int id, CancellationToken cancellationToken = default)
        {
            var issues = await _magazineService.GetIssuesByMagazineIdAsync(id, cancellationToken);
            return Ok(issues);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] MagazineWriteDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var createdMagazine = await _magazineService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdMagazine.Id }, createdMagazine);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] MagazineWriteDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _magazineService.UpdateAsync(id, request, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            await _magazineService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}