using System.Threading;
using System.Threading.Tasks;
using Torath.Entities;
using Torath.DTOs;

namespace Torath.Services
{
    public interface IBookService
    {
        Task<object> GetAllAsync(int page, int pageSize, string? category, string? language, CancellationToken cancellationToken);
        Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<Book> CreateAsync(BookWriteDto request, CancellationToken cancellationToken);
        Task UpdateAsync(int id, BookWriteDto request, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}