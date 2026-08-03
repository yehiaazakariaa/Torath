using Microsoft.EntityFrameworkCore;
using Torath.DTOs;
using Torath.Entities;
using Torath.SearchModels; // 1. Added for SearchDocument
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

        // 2. Inject Elasticsearch Service
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
                    PdfFileUrl = rp.PdfFileUrl
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
                PdfFileUrl = paper.PdfFileUrl
            };
        }

        public async Task<ResearchPaperDto> CreateAsync(ResearchPaperWriteDto request)
        {
            // 3. Save to SQL
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

            // 4. Map to Elasticsearch Document
            var searchDoc = new SearchDocument
            {
                Id = $"ResearchPaper_{paper.Id}",
                OriginalId = paper.Id,
                Title = paper.Title,
                Description = paper.Abstract, // Map the 'Abstract' field to the unified 'Description'
                Content = "", // PDF content would require a text-extractor. Left empty for now.
                Author = paper.Author,
                Category = paper.CategoryId.ToString(),
                Publisher = "", // Research papers in your model don't have publishers
                Language = "", // Assuming language isn't tracked here
                Keywords = new List<string>(),
                // Convert the PublicationYear integer into a DateTime for the unified model
                PublicationDate = new DateTime(paper.PublicationYear, 1, 1),
                ContentType = "Research Paper"
            };

            // 5. Index the document
            await _elasticService.IndexDocumentAsync(searchDoc);

            return await GetByIdAsync(paper.Id);
        }

        public async Task<bool> UpdateAsync(int id, ResearchPaperWriteDto request)
        {
            var paper = await _context.ResearchPapers.FindAsync(id);
            if (paper == null) return false;

            // 6. Update SQL
            paper.Title = request.Title;
            paper.Abstract = request.Abstract;
            paper.Author = request.Author;
            paper.PublicationYear = request.PublicationYear;
            paper.CategoryId = request.CategoryId;
            paper.CoverImageUrl = request.CoverImageUrl;
            paper.PdfFileUrl = request.PdfFileUrl;

            await _context.SaveChangesAsync();

            // 7. Sync update to Elasticsearch
            var searchDoc = new SearchDocument
            {
                Id = $"ResearchPaper_{paper.Id}",
                OriginalId = paper.Id,
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

            // 8. Delete from SQL
            _context.ResearchPapers.Remove(paper);
            await _context.SaveChangesAsync();

            // 9. Delete from Elasticsearch
            await _elasticService.DeleteDocumentAsync($"ResearchPaper_{id}");

            return true;
        }
    }
}