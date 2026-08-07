using Microsoft.EntityFrameworkCore;
using Torath.DTOs;
using Torath.Entities;
using Torath.SearchModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Torath.Services
{
    public class ResearchPaperService : IResearchPaperService
    {
        private readonly TorathDbContext _context;
        private readonly IFileService _fileService;
        private readonly IElasticSearchService _elasticService;

        public ResearchPaperService(TorathDbContext context, IFileService fileService, IElasticSearchService elasticService)
        {
            _context = context;
            _fileService = fileService;
            _elasticService = elasticService;
        }

        public async Task<PagedResponse<ResearchPaperDto>> GetAllAsync(int page, int pageSize, int? publicationYear)
        {
            var query = _context.ResearchPapers.Include(rp => rp.Category).AsQueryable();

            if (publicationYear.HasValue)
                query = query.Where(rp => rp.PublicationYear == publicationYear.Value);

            var totalRecords = await query.CountAsync();
            var papers = await query.OrderByDescending(rp => rp.Id).Skip((page - 1) * pageSize).Take(pageSize)
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
                    PdfFileUrl = rp.PdfFileUrl,
                    Rating = rp.Rating,
                    ViewCount = rp.ViewCount
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
                PdfFileUrl = paper.PdfFileUrl,
                Rating = paper.Rating,
                ViewCount = paper.ViewCount
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
                PdfFileUrl = request.PdfFileUrl
            };

            _context.ResearchPapers.Add(paper);
            await _context.SaveChangesAsync();

            var searchDoc = new SearchDocument
            {
                Id = $"ResearchPaper_{paper.Id}",
                OriginalId = paper.Id,
                DatabaseId = paper.Id, // Mapped for frontend routing
                Title = paper.Title,
                Description = paper.Abstract,
                Content = "",
                Author = paper.Author,
                Category = paper.CategoryId.ToString(),
                Publisher = "",
                Language = "",
                Keywords = new List<string>(),
                PublicationDate = new DateTime(paper.PublicationYear, 1, 1),
                ContentType = "Research Paper"
            };

            await _elasticService.IndexDocumentAsync(searchDoc);

            return await GetByIdAsync(paper.Id);
        }

        public async Task<bool> UpdateAsync(int id, ResearchPaperWriteDto request)
        {
            var paper = await _context.ResearchPapers.FindAsync(id);
            if (paper == null) return false;

            paper.Title = request.Title;
            paper.Abstract = request.Abstract;
            paper.Author = request.Author;
            paper.PublicationYear = request.PublicationYear;
            paper.CategoryId = request.CategoryId;
            paper.CoverImageUrl = request.CoverImageUrl;
            paper.PdfFileUrl = request.PdfFileUrl;

            await _context.SaveChangesAsync();

            var searchDoc = new SearchDocument
            {
                Id = $"ResearchPaper_{paper.Id}",
                OriginalId = paper.Id,
                DatabaseId = paper.Id, // Mapped for frontend routing
                Title = paper.Title,
                Description = paper.Abstract,
                Content = "",
                Author = paper.Author,
                Category = paper.CategoryId.ToString(),
                Publisher = "",
                Language = "",
                Keywords = new List<string>(),
                PublicationDate = new DateTime(paper.PublicationYear, 1, 1),
                ContentType = "Research Paper"
            };

            await _elasticService.IndexDocumentAsync(searchDoc);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var paper = await _context.ResearchPapers.FindAsync(id);
            if (paper == null) return false;

            _fileService.DeleteFile(paper.CoverImageUrl);
            _fileService.DeleteFile(paper.PdfFileUrl);

            _context.ResearchPapers.Remove(paper);
            await _context.SaveChangesAsync();

            await _elasticService.DeleteDocumentAsync($"ResearchPaper_{id}");

            return true;
        }
    }
}