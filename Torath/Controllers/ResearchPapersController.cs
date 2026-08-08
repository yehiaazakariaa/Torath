using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Torath.DTOs;
using Torath.Services;

namespace Torath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResearchPapersController : ControllerBase
    {
        private readonly IResearchPaperService _researchPaperService;

        public ResearchPapersController(IResearchPaperService researchPaperService)
        {
            _researchPaperService = researchPaperService;
        }

        [HttpGet]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? publicationYear = null)
        {
            return Ok(await _researchPaperService.GetAllAsync(page, pageSize, publicationYear));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var paper = await _researchPaperService.GetByIdAsync(id);
            return paper == null ? NotFound() : Ok(paper);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ResearchPaperWriteDto request)
        {
            var paper = await _researchPaperService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = paper.Id }, paper);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, ResearchPaperWriteDto request)
        {
            return await _researchPaperService.UpdateAsync(id, request) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            return await _researchPaperService.DeleteAsync(id) ? NoContent() : NotFound();
        }

        [HttpPost("{id}/view")]
        [AllowAnonymous]
        public async Task<IActionResult> IncrementViewCount(int id)
        {
            await _researchPaperService.IncrementViewCountAsync(id);
            return Ok();
        }

        [HttpPost("{id}/rate")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> RateResearchPaper(int id, [FromBody] double rating)
        {
            if (rating < 0 || rating > 5) return BadRequest("Rating must be between 0 and 5.");

            await _researchPaperService.UpdateRatingAsync(id, rating);
            return Ok();
        }
    }
}