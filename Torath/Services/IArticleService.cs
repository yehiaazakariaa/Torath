using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Torath.DTOs;

namespace Torath.Services
{
    public interface IArticleService
    {
        Task<object> GetAllAsync(int page, int pageSize, string? author, CancellationToken cancellationToken);
        Task<ArticleDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<ArticleDto> CreateAsync(ArticleWriteDto request, CancellationToken cancellationToken);
        Task UpdateAsync(int id, ArticleWriteDto request, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);

        Task IncrementViewCountAsync(int id, CancellationToken cancellationToken);
        Task UpdateRatingAsync(int id, double rating, CancellationToken cancellationToken);
    }
}