using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Torath.Entities;
using Torath.DTOs;
using Torath.Repositories;
using Torath.SearchModels;

namespace Torath.Services
{
    public class MagazineService : IMagazineService
    {
        private readonly IRepository<Magazine> _magazineRepository;
        private readonly IRepository<MagazineIssue> _issueRepository;
        private readonly IElasticSearchService _elasticService;

        public MagazineService(IRepository<Magazine> magazineRepository, IRepository<MagazineIssue> issueRepository, IElasticSearchService elasticService)
        {
            _magazineRepository = magazineRepository;
            _issueRepository = issueRepository;
            _elasticService = elasticService;
        }

        public async Task<object> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            var query = _magazineRepository.GetQueryable().Include(m => m.Category);

            var totalRecords = await query.CountAsync(cancellationToken);
            var data = await query
                .OrderByDescending(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MagazineDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    Language = m.Language,
                    Publisher = m.Publisher,
                    PublicationDate = m.PublicationDate,
                    CategoryId = m.CategoryId,
                    CategoryName = m.Category != null ? m.Category.Name : string.Empty,
                    ISSN = m.ISSN,
                    Rating = m.Rating,
                    ViewCount = m.ViewCount,
                    CoverImageUrl = m.CoverImageUrl,
                    PdfFileUrl = m.PdfFileUrl,
                })
                .ToListAsync(cancellationToken);

            return new { data, totalRecords, pageNumber = page, pageSize };
        }

        public async Task<MagazineDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var m = await _magazineRepository.GetQueryable()
                .Include(mg => mg.Category)
                .FirstOrDefaultAsync(mg => mg.Id == id, cancellationToken);
            if (m == null) return null;

            return new MagazineDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Language = m.Language,
                Publisher = m.Publisher,
                PublicationDate = m.PublicationDate,
                CategoryId = m.CategoryId,
                CategoryName = m.Category != null ? m.Category.Name : string.Empty,
                ISSN = m.ISSN,
                Rating = m.Rating,
                ViewCount = m.ViewCount,
                CoverImageUrl = m.CoverImageUrl,
                PdfFileUrl = m.PdfFileUrl,
            };
        }

        public async Task<IEnumerable<MagazineIssueDto>> GetIssuesByMagazineIdAsync(int magazineId, CancellationToken cancellationToken)
        {
            return await _issueRepository.GetQueryable()
                .Where(i => i.MagazineId == magazineId)
                .Select(i => new MagazineIssueDto
                {
                    Id = i.Id,
                    IssueNumber = i.IssueNumber,
                    VolumeNumber = i.VolumeNumber,
                    PublicationDate = i.PublicationDate,
                    MagazineId = i.MagazineId,
                    Rating = i.Rating,
                    ViewCount = i.ViewCount
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<MagazineDto> CreateAsync(MagazineWriteDto request, CancellationToken cancellationToken)
        {
            var magazine = new Magazine
            {
                Title = request.Title,
                Description = request.Description,
                Language = request.Language,
                PublicationDate = request.PublicationDate,
                Publisher = request.Publisher,
                CategoryId = request.CategoryId,
                ISSN = request.ISSN,
                CoverImageUrl = request.CoverImageUrl,
                PdfFileUrl = request.PdfFileUrl
            };

            await _magazineRepository.AddAsync(magazine, cancellationToken);
            await _magazineRepository.SaveChangesAsync(cancellationToken);

            var searchDoc = new SearchDocument
            {
                Id = $"Magazine_{magazine.Id}",
                OriginalId = magazine.Id,
                DatabaseId = magazine.Id,
                Title = magazine.Title,
                Description = magazine.Description,
                Content = "",
                Author = "",
                Category = magazine.CategoryId.ToString(),
                Publisher = magazine.Publisher,
                Language = magazine.Language,
                Keywords = new List<string>(),
                PublicationDate = magazine.PublicationDate,
                ContentType = "Magazine"
            };

            await _elasticService.IndexDocumentAsync(searchDoc);

            return await GetByIdAsync(magazine.Id, cancellationToken) ?? throw new Exception("Failed to return created magazine.");
        }

        public async Task UpdateAsync(int id, MagazineWriteDto request, CancellationToken cancellationToken)
        {
            var magazine = await _magazineRepository.GetByIdAsync(id, cancellationToken);
            if (magazine == null) throw new Exception($"Magazine with ID {id} not found.");

            magazine.Title = request.Title;
            magazine.Description = request.Description;
            magazine.Language = request.Language;
            magazine.PublicationDate = request.PublicationDate;
            magazine.Publisher = request.Publisher;
            magazine.CategoryId = request.CategoryId;
            magazine.ISSN = request.ISSN;
            magazine.CoverImageUrl = request.CoverImageUrl;
            magazine.PdfFileUrl = request.PdfFileUrl;

            _magazineRepository.Update(magazine);
            await _magazineRepository.SaveChangesAsync(cancellationToken);

            var searchDoc = new SearchDocument
            {
                Id = $"Magazine_{magazine.Id}",
                OriginalId = magazine.Id,
                DatabaseId = magazine.Id,
                Title = magazine.Title,
                Description = magazine.Description,
                Content = "",
                Author = "",
                Category = magazine.CategoryId.ToString(),
                Publisher = magazine.Publisher,
                Language = magazine.Language,
                Keywords = new List<string>(),
                PublicationDate = magazine.PublicationDate,
                ContentType = "Magazine"
            };

            await _elasticService.IndexDocumentAsync(searchDoc);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var magazine = await _magazineRepository.GetByIdAsync(id, cancellationToken);
            if (magazine != null)
            {
                _magazineRepository.Delete(magazine);
                await _magazineRepository.SaveChangesAsync(cancellationToken);
                await _elasticService.DeleteDocumentAsync($"Magazine_{id}");
            }
        }

        public async Task IncrementViewCountAsync(int id, CancellationToken cancellationToken)
        {
            var magazine = await _magazineRepository.GetByIdAsync(id, cancellationToken);
            if (magazine != null)
            {
                magazine.ViewCount += 1;
                _magazineRepository.Update(magazine);
                await _magazineRepository.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task UpdateRatingAsync(int id, double rating, CancellationToken cancellationToken)
        {
            var magazine = await _magazineRepository.GetByIdAsync(id, cancellationToken);
            if (magazine != null)
            {
                magazine.Rating = rating;
                _magazineRepository.Update(magazine);
                await _magazineRepository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}