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
            // Included Category to prevent null reference on the frontend
            var query = _magazineRepository.GetQueryable().Include(m => m.Category);

            var totalRecords = await query.CountAsync(cancellationToken);
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new { data, totalRecords, pageNumber = page, pageSize };
        }

        public async Task<Magazine?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _magazineRepository.GetQueryable()
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<MagazineIssue>> GetIssuesByMagazineIdAsync(int magazineId, CancellationToken cancellationToken)
        {
            return await _issueRepository.GetQueryable()
                .Where(i => i.MagazineId == magazineId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Magazine> CreateAsync(MagazineWriteDto request, CancellationToken cancellationToken)
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
            };

            await _magazineRepository.AddAsync(magazine, cancellationToken);
            await _magazineRepository.SaveChangesAsync(cancellationToken);

            var searchDoc = new SearchDocument
            {
                Id = $"Magazine_{magazine.Id}",
                OriginalId = magazine.Id,
                DatabaseId = magazine.Id, // Mapped for frontend
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

            return magazine;
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

            _magazineRepository.Update(magazine);
            await _magazineRepository.SaveChangesAsync(cancellationToken);

            var searchDoc = new SearchDocument
            {
                Id = $"Magazine_{magazine.Id}",
                OriginalId = magazine.Id,
                DatabaseId = magazine.Id, // Mapped for frontend
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
    }
}