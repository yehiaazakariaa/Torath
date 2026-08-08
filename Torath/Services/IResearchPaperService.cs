using Torath.DTOs;

namespace Torath.Services
{
    public interface IResearchPaperService
    {
        Task<PagedResponse<ResearchPaperDto>> GetAllAsync(int page, int pageSize, int? publicationYear);
        Task<ResearchPaperDto?> GetByIdAsync(int id);
        Task<ResearchPaperDto> CreateAsync(ResearchPaperWriteDto request);
        Task<bool> UpdateAsync(int id, ResearchPaperWriteDto request);
        Task<bool> DeleteAsync(int id);
        Task IncrementViewCountAsync(int id);
        Task UpdateRatingAsync(int id, double rating);
    }
}