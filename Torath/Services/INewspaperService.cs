using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Torath.DTOs;

namespace Torath.Services
{
    public interface INewspaperService
    {
        Task<object> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
        Task<NewspaperReadDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<IEnumerable<NewspaperIssueDto>> GetIssuesByNewspaperIdAsync(int newspaperId, CancellationToken cancellationToken);
        Task<NewspaperReadDto> CreateAsync(NewspaperWriteDto request, CancellationToken cancellationToken);
        Task UpdateAsync(int id, NewspaperWriteDto request, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);

        Task IncrementViewCountAsync(int id, CancellationToken cancellationToken);
        Task UpdateRatingAsync(int id, double rating, CancellationToken cancellationToken);
    }
}