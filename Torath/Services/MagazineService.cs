using System;
using System.Collections.Generic; // Added for List<string> Keywords
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Torath.Entities;
using Torath.DTOs;
using Torath.Repositories;
using Torath.SearchModels; // 1. Added to access the SearchDocument model

namespace Torath.Services
{
    public class MagazineService : IMagazineService
    {
        private readonly IRepository<Magazine> _magazineRepository;
        private readonly IRepository<MagazineIssue> _issueRepository;

        // 2. Inject the Elasticsearch service
        private readonly IElasticSearchService _elasticService;

        public MagazineService(IRepository<Magazine> magazineRepository, IRepository<MagazineIssue> issueRepository, IElasticSearchService elasticService)
        {
            _magazineRepository = magazineRepository;
            _issueRepository = issueRepository;
            _elasticService = elasticService;
        }

        public async Task<object> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            var query = _magazineRepository.GetQueryable();
            var totalRecords = await query.CountAsync(cancellationToken);
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new { data, totalRecords, pageNumber = page, pageSize };
        }

        public async Task<Magazine?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _magazineRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IEnumerable<MagazineIssue>> GetIssuesByMagazineIdAsync(int magazineId, CancellationToken cancellationToken)
        {
            return await _issueRepository.GetQueryable()
                .Where(i => i.MagazineId == magazineId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Magazine> CreateAsync(MagazineWriteDto request, CancellationToken cancellationToken)
        {
            // 3. Save the Magazine to SQL Server normally
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

            // 4. Map the newly created Magazine to the unified Elasticsearch Document
            var searchDoc = new SearchDocument
            {
                Id = $"Magazine_{magazine.Id}", // Uses the "Magazine" prefix to avoid ID collisions with Books
                OriginalId = magazine.Id,
                Title = magazine.Title,
                Description = magazine.Description,
                Content = "",
                Author = "", // Magazines usually don't have a single author; leaving blank or you could use Publisher here
                Category = magazine.CategoryId.ToString(),
                Publisher = magazine.Publisher,
                Language = magazine.Language,
                Keywords = new List<string>(),
                PublicationDate = magazine.PublicationDate,
                ContentType = "Magazine" // Explicitly marks this as a Magazine for search filtering
            };

            // 5. Send it to the Elasticsearch Index
            await _elasticService.IndexDocumentAsync(searchDoc);

            return magazine;
        }

        public async Task UpdateAsync(int id, MagazineWriteDto request, CancellationToken cancellationToken)
        {
            var magazine = await _magazineRepository.GetByIdAsync(id, cancellationToken);
            if (magazine == null) throw new Exception($"Magazine with ID {id} not found.");

            // 6. Update SQL Server normally
            magazine.Title = request.Title;
            magazine.Description = request.Description;
            magazine.Language = request.Language;
            magazine.PublicationDate = request.PublicationDate;
            magazine.Publisher = request.Publisher;
            magazine.CategoryId = request.CategoryId;
            magazine.ISSN = request.ISSN;

            _magazineRepository.Update(magazine);
            await _magazineRepository.SaveChangesAsync(cancellationToken);

            // 7. Sync the update to Elasticsearch. 
            var searchDoc = new SearchDocument
            {
                Id = $"Magazine_{magazine.Id}",
                OriginalId = magazine.Id,
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
                // 8. Delete from SQL Server
                _magazineRepository.Delete(magazine);
                await _magazineRepository.SaveChangesAsync(cancellationToken);

                // 9. Remove the corresponding document from Elasticsearch using the "Magazine" prefix
                await _elasticService.DeleteDocumentAsync($"Magazine_{id}");
            }
        }
    }
}