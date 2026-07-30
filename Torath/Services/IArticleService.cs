using System.Threading;
using System.Threading.Tasks;
using Torath.Entities;
using Torath.DTOs;

namespace Torath.Services
{
    public interface IArticleService
    {
        // Add CancellationToken to all signatures
        Task<object> GetAllAsync(int page, int pageSize, string? author, CancellationToken cancellationToken);
        Task<Article> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<Article> CreateAsync(ArticleWriteDto request, CancellationToken cancellationToken);
        Task UpdateAsync(int id, ArticleWriteDto request, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}