using Microsoft.EntityFrameworkCore;
using Torath.DTOs;
using Torath.Entities;

namespace Torath.Services
{
    public class ResearchPaperService : IResearchPaperService
    {
        private readonly TorathDbContext _context;
        private readonly IFileService _fileService;
        public ResearchPaperService(TorathDbContext context, IFileService fileService) // Update constructor
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<PagedResponse<ResearchPaperDto>> GetAllAsync(int page, int pageSize, int? publicationYear)
        {
            var query = _context.ResearchPapers.Include(rp => rp.Category).AsQueryable();

            if (publicationYear.HasValue)
                query = query.Where(rp => rp.PublicationYear == publicationYear.Value);

            var totalRecords = await query.CountAsync();
            var papers = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(rp => new ResearchPaperDto
                {
                    Id = rp.Id,
                    Title = rp.Title,
                    Abstract = rp.Abstract,
                    Author = rp.Author,
                    PublicationYear = rp.PublicationYear,
                    CategoryId = rp.CategoryId,
                    CategoryName = rp.Category.Name,
                    CoverImageUrl = rp.CoverImageUrl,
                    PdfFileUrl = rp.PdfFileUrl // Added
                }).ToListAsync();

            return new PagedResponse<ResearchPaperDto> { Data = papers, TotalRecords = totalRecords, PageNumber = page, PageSize = pageSize };
        }

        public async Task<ResearchPaperDto?> GetByIdAsync(int id)
        {
            var paper = await _context.ResearchPapers.Include(rp => rp.Category).FirstOrDefaultAsync(rp => rp.Id == id);
            if (paper == null) return null;

            return new ResearchPaperDto
            {
                Id = paper.Id,
                Title = paper.Title,
                Abstract = paper.Abstract,
                Author = paper.Author,
                PublicationYear = paper.PublicationYear,
                CategoryId = paper.CategoryId,
                CategoryName = paper.Category.Name,
                CoverImageUrl = paper.CoverImageUrl,
                PdfFileUrl = paper.PdfFileUrl // Added
            };
        }

        public async Task<ResearchPaperDto> CreateAsync(ResearchPaperWriteDto request)
        {
            var paper = new ResearchPaper
            {
                Title = request.Title,
                Abstract = request.Abstract,
                Author = request.Author,
                PublicationYear = request.PublicationYear,
                CategoryId = request.CategoryId,
                CoverImageUrl = request.CoverImageUrl,
                PdfFileUrl = request.PdfFileUrl // Added
            };

            _context.ResearchPapers.Add(paper);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(paper.Id);
        }

        public async Task<bool> UpdateAsync(int id, ResearchPaperWriteDto request)
        {
            var paper = await _context.ResearchPapers.FindAsync(id);
            if (paper == null) return false;

            paper.Title = request.Title; paper.Abstract = request.Abstract; paper.Author = request.Author;
            paper.PublicationYear = request.PublicationYear; paper.CategoryId = request.CategoryId;
            paper.CoverImageUrl = request.CoverImageUrl; paper.PdfFileUrl = request.PdfFileUrl; // Added

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var paper = await _context.ResearchPapers.FindAsync(id);
            if (paper == null) return false;

            // Delete associated physical files
            _fileService.DeleteFile(paper.CoverImageUrl);
            _fileService.DeleteFile(paper.PdfFileUrl);

            _context.ResearchPapers.Remove(paper);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}