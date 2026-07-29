using Microsoft.EntityFrameworkCore;
using Torath.DTOs;
using Torath.Entities;

namespace Torath.Services
{
    public class MagazineService : IMagazineService
    {
        private readonly TorathDbContext _context;

        public MagazineService(TorathDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<MagazineDto>> GetAllAsync(int page, int pageSize)
        {
            var query = _context.Magazines.Include(m => m.Category).AsQueryable();
            var totalRecords = await query.CountAsync();

            var magazines = await query.Skip((page - 1) * pageSize).Take(pageSize)
              .Select(m => new MagazineDto
              {
                  Id = m.Id,
                  Title = m.Title,
                  Description = m.Description,
                  Language = m.Language,
                  Publisher = m.Publisher,
                  PublicationDate = m.PublicationDate, // FIXED
                  CategoryId = m.CategoryId,
                  CategoryName = m.Category.Name,
                  ISSN = m.ISSN
              }).ToListAsync();

            return new PagedResponse<MagazineDto> { Data = magazines, TotalRecords = totalRecords, PageNumber = page, PageSize = pageSize };
        }

        public async Task<MagazineDto?> GetByIdAsync(int id)
        {
            var magazine = await _context.Magazines.Include(m => m.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (magazine == null) return null;

            return new MagazineDto
            {
                Id = magazine.Id,
                Title = magazine.Title,
                Description = magazine.Description,
                Language = magazine.Language,
                Publisher = magazine.Publisher,
                PublicationDate = magazine.PublicationDate, // FIXED
                CategoryId = magazine.CategoryId,
                CategoryName = magazine.Category.Name,
                ISSN = magazine.ISSN
            };
        }

        // --- The Nested Issues Method ---
        public async Task<IEnumerable<MagazineIssueDto>> GetIssuesByMagazineIdAsync(int magazineId)
        {
            return await _context.MagazineIssues
                .Where(i => i.MagazineId == magazineId)
                .Select(i => new MagazineIssueDto
                {
                    Id = i.Id,
                    IssueNumber = i.IssueNumber,
                    VolumeNumber = i.VolumeNumber,
                    PublicationDate = i.PublicationDate,
                    MagazineId = i.MagazineId
                }).ToListAsync();
        }

        public async Task<MagazineDto> CreateAsync(MagazineWriteDto request)
        {
            var magazine = new Magazine
            {
                Title = request.Title,
                Description = request.Description,
                Language = request.Language,
                Publisher = request.Publisher,
                PublicationDate = request.PublicationDate, // FIXED
                CategoryId = request.CategoryId,
                ISSN = request.ISSN
            };

            _context.Magazines.Add(magazine);
            await _context.SaveChangesAsync();

            // FIXED: The '!' tells the compiler this will not be null
            return (await GetByIdAsync(magazine.Id))!;
        }

        public async Task<bool> UpdateAsync(int id, MagazineWriteDto request)
        {
            var magazine = await _context.Magazines.FindAsync(id);
            if (magazine == null) return false;

            magazine.Title = request.Title;
            magazine.Description = request.Description;
            magazine.Language = request.Language;
            magazine.Publisher = request.Publisher;
            magazine.PublicationDate = request.PublicationDate; // FIXED
            magazine.CategoryId = request.CategoryId;
            magazine.ISSN = request.ISSN;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var magazine = await _context.Magazines.FindAsync(id);
            if (magazine == null) return false;

            _context.Magazines.Remove(magazine);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}