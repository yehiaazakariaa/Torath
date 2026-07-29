using System.Collections.Generic;
using System.Threading.Tasks;
using Torath.DTOs;
using Torath.Entities;


namespace Torath.Services
{
    public interface INewspaperService
    {
        Task<object> GetAllAsync(int page, int pageSize);
        Task<Newspaper> GetByIdAsync(int id);
        Task<IEnumerable<NewspaperIssue>> GetIssuesByNewspaperIdAsync(int newspaperId);
        Task<Newspaper> CreateAsync(NewspaperWriteDto request);
        Task UpdateAsync(int id, NewspaperWriteDto request);
        Task DeleteAsync(int id);
    }
}