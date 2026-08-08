using System.Threading;
using System.Threading.Tasks;
using Torath.Entities;
using Torath.DTOs;

namespace Torath.Services
{
    public interface IBookService
    {
        Task<object> GetAllAsync(int page, int pageSize, string? category, string? language, CancellationToken cancellationToken);
        Task<BookDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<BookDto> CreateAsync(BookWriteDto request, CancellationToken cancellationToken);
        Task UpdateAsync(int id, BookWriteDto request, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
        Task IncrementViewCountAsync(int id, CancellationToken cancellationToken);
        Task UpdateRatingAsync(int id, double rating, CancellationToken cancellationToken);
    }
}