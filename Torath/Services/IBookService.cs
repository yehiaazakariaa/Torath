using Torath.DTOs;

namespace Torath.Services
{
    public interface IBookService
    {
        Task<PagedResponse<BookDto>> GetAllAsync(int page, int pageSize, string? category, string? language);
        Task<BookDto?> GetByIdAsync(int id);
        Task<BookDto> CreateAsync(BookWriteDto request);
        Task<bool> UpdateAsync(int id, BookWriteDto request);
        Task<bool> DeleteAsync(int id);
    }
}