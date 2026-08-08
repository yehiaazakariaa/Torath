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
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
      
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? category = null, [FromQuery] string? language = null, CancellationToken cancellationToken = default)
        {
            var result = await _bookService.GetAllAsync(page, pageSize, category, language, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
        {
            var book = await _bookService.GetByIdAsync(id, cancellationToken);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] BookWriteDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var createdBook = await _bookService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdBook.Id }, createdBook);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] BookWriteDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _bookService.UpdateAsync(id, request, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            await _bookService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }


        [HttpPost("{id}/view")]
        [AllowAnonymous] // Anyone viewing the card/page increments the view
        public async Task<IActionResult> IncrementViewCount(int id, CancellationToken cancellationToken = default)
        {
            await _bookService.IncrementViewCountAsync(id, cancellationToken);
            return Ok();
        }

        [HttpPost("{id}/rate")]
        [Authorize(Roles = "User, Admin")] // Only logged in users can rate
        public async Task<IActionResult> RateBook(int id, [FromBody] double rating, CancellationToken cancellationToken = default)
        {
            if (rating < 0 || rating > 5)
            {
                return BadRequest("Rating must be between 0 and 5.");
            }

            await _bookService.UpdateRatingAsync(id, rating, cancellationToken);
            return Ok();
        }

    }
}