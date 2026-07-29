using System.Collections.Generic;
using System.Threading.Tasks;
using Torath.Entities; 
using Torath.DTOs;

namespace Torath.Services
{
    public interface IMagazineIssueService
    {
        // Returns an anonymous object containing the pagination metadata and data array
        Task<object> GetAllAsync(int page, int pageSize);

        Task<MagazineIssue> GetByIdAsync(int id);

        // Nested endpoint requirement: Retrieves all articles for a specific issue
        Task<IEnumerable<Article>> GetArticlesByIssueIdAsync(int issueId);

        Task<MagazineIssue> CreateAsync(MagazineIssueWriteDto request);

        Task UpdateAsync(int id, MagazineIssueWriteDto request);

        Task DeleteAsync(int id);
    }
}