using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Torath.Entities;
using Torath.DTOs;

namespace Torath.Services
{
    public interface INewspaperService
    {
        Task<object> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
        Task<Newspaper?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<IEnumerable<NewspaperIssue>> GetIssuesByNewspaperIdAsync(int newspaperId, CancellationToken cancellationToken);
        Task<Newspaper> CreateAsync(NewspaperWriteDto request, CancellationToken cancellationToken);
        Task UpdateAsync(int id, NewspaperWriteDto request, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}