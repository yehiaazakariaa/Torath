using System.Threading.Tasks;
using Torath.Entities;
using Torath.DTOs;

namespace Torath.Services
{
    public interface IArticleService
    {
        // Notice the added 'author' parameter for filtering[cite: 1]
        Task<object> GetAllAsync(int page, int pageSize, string? author);

        Task<Article> GetByIdAsync(int id);

        Task<Article> CreateAsync(ArticleWriteDto request);

        Task UpdateAsync(int id, ArticleWriteDto request);

        Task DeleteAsync(int id);
    }
}