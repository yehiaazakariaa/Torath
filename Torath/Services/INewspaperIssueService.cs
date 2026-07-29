using System.Collections.Generic;
using System.Threading.Tasks;
using Torath.Entities;
using Torath.DTOs;

namespace Torath.Services
{
    public interface INewspaperIssueService
    {
        Task<object> GetAllAsync(int page, int pageSize);                  // Retrieves paginated list of issues
        Task<NewspaperIssue> GetByIdAsync(int id);                         // Retrieves a single issue by its ID
        Task<IEnumerable<Article>> GetArticlesByIssueIdAsync(int issueId); // Nested endpoint: Retrieves articles belonging to this issue
        Task<NewspaperIssue> CreateAsync(NewspaperIssueWriteDto request);  // Maps DTO to Entity and saves to database
        Task UpdateAsync(int id, NewspaperIssueWriteDto request);          // Updates an existing issue
        Task DeleteAsync(int id);                                          // Removes an issue from the database
    }
}