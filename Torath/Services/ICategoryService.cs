using System.Threading;
using System.Threading.Tasks;
using Torath.Entities;
using Torath.DTOs;

namespace Torath.Services
{
    public interface ICategoryService
    {
        Task<object> GetAllAsync(int page, int pageSize, string? search, CancellationToken cancellationToken);
        Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<Category> CreateAsync(CategoryWriteDto request, CancellationToken cancellationToken);
        Task UpdateAsync(int id, CategoryWriteDto request, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}