using Torath.DTOs;

namespace Torath.Services
{
    public interface ICategoryService
    {
        Task<PagedResponse<CategoryDto>> GetAllAsync(int pageNumber, int pageSize, string? search);
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CategoryWriteDto request);
        Task<bool> UpdateAsync(int id, CategoryWriteDto request);
        Task<bool> DeleteAsync(int id);
    }
}