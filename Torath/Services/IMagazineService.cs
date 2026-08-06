using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Torath.DTOs;

namespace Torath.Services
{
    public interface IMagazineService
    {
        Task<object> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
        Task<MagazineDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<IEnumerable<MagazineIssueDto>> GetIssuesByMagazineIdAsync(int magazineId, CancellationToken cancellationToken);
        Task<MagazineDto> CreateAsync(MagazineWriteDto request, CancellationToken cancellationToken);
        Task UpdateAsync(int id, MagazineWriteDto request, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}