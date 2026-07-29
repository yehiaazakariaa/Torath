using Torath.DTOs;

namespace Torath.Services
{
    public interface IMagazineService
    {
        Task<PagedResponse<MagazineDto>> GetAllAsync(int page, int pageSize);
        Task<MagazineDto?> GetByIdAsync(int id);
        Task<IEnumerable<MagazineIssueDto>> GetIssuesByMagazineIdAsync(int magazineId);
        Task<MagazineDto> CreateAsync(MagazineWriteDto request);
        Task<bool> UpdateAsync(int id, MagazineWriteDto request);
        Task<bool> DeleteAsync(int id);
    }
}