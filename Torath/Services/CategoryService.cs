using Microsoft.EntityFrameworkCore;
using Torath.DTOs;
using Torath.Entities;

namespace Torath.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly TorathDbContext _context;

        public CategoryService(TorathDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<CategoryDto>> GetAllAsync(int pageNumber, int pageSize, string? search)
        {
            var query = _context.Categories.AsQueryable();

            // 1. Apply Filtering if a search term exists
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.Contains(search) || c.Description.Contains(search));
            }

            // 2. Count total records BEFORE paginating (needed for the frontend)
            var totalRecords = await query.CountAsync();

            // 3. Apply Pagination (Skip and Take)
            var categories = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync();

            return new PagedResponse<CategoryDto>
            {
                Data = categories,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryDto> CreateAsync(CategoryWriteDto request)
        {
            var category = new Category
            {
                Name = request.Name,
                Description = request.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<bool> UpdateAsync(int id, CategoryWriteDto request)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            category.Name = request.Name;
            category.Description = request.Description;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}