using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Torath.DTOs;
using Torath.Services;

namespace Torath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] <-- Uncomment this later to lock down the entire controller
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _categoryService.GetAllAsync(page, pageSize, search);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound("Category not found.");

            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryWriteDto request)
        {
            var createdCategory = await _categoryService.CreateAsync(request);

            // Returns a 201 Created status and points to the GetById route
            return CreatedAtAction(nameof(GetById), new { id = createdCategory.Id }, createdCategory);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CategoryWriteDto request)
        {
            var success = await _categoryService.UpdateAsync(id, request);
            if (!success) return NotFound("Category not found.");

            return NoContent(); // 204 No Content is standard for successful PUT
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _categoryService.DeleteAsync(id);
            if (!success) return NotFound("Category not found.");

            return NoContent();
        }
    }
}