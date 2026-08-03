using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Torath.Entities;
using Torath.DTOs;
using Torath.Repositories;
using Torath.SearchModels; // 1. Added for SearchDocument

namespace Torath.Services
{
    public class NewspaperService : INewspaperService
    {
        private readonly IRepository<Newspaper> _newspaperRepository;
        private readonly IRepository<NewspaperIssue> _issueRepository;

        // 2. Inject Elasticsearch Service
        private readonly IElasticSearchService _elasticService;

        public NewspaperService(IRepository<Newspaper> newspaperRepository, IRepository<NewspaperIssue> issueRepository, IElasticSearchService elasticService)
        {
            _newspaperRepository = newspaperRepository;
            _issueRepository = issueRepository;
            _elasticService = elasticService;
        }

        public async Task<object> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            var query = _newspaperRepository.GetQueryable();
            var totalRecords = await query.CountAsync(cancellationToken);
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new { data, totalRecords, pageNumber = page, pageSize };
        }

        public async Task<Newspaper?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _newspaperRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IEnumerable<NewspaperIssue>> GetIssuesByNewspaperIdAsync(int newspaperId, CancellationToken cancellationToken)
        {
            return await _issueRepository.GetQueryable()
                .Where(i => i.NewspaperId == newspaperId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Newspaper> CreateAsync(NewspaperWriteDto request, CancellationToken cancellationToken)
        {
            // 3. Save to SQL Server
            var newspaper = new Newspaper
            {
                Title = request.Title,
                Description = request.Description,
                Language = request.Language,
                PublicationDate = request.PublicationDate,
                Publisher = request.Publisher,
                CategoryId = request.CategoryId,
                Frequency = request.Frequency,
                Price = request.Price,
                PdfFilePath = request.PdfFilePath
            };

            await _newspaperRepository.AddAsync(newspaper, cancellationToken);
            await _newspaperRepository.SaveChangesAsync(cancellationToken);

            // 4. Map to Elasticsearch Document
            var searchDoc = new SearchDocument
            {
                Id = $"Newspaper_{newspaper.Id}", // Unique ID for Newspapers
                OriginalId = newspaper.Id,
                Title = newspaper.Title,
                Description = newspaper.Description,
                Content = "", // No massive raw text body typically stored here
                Author = "", // Newspapers generally use 'Publisher' rather than a single 'Author'
                Category = newspaper.CategoryId.ToString(),
                Publisher = newspaper.Publisher,
                Language = newspaper.Language,
                Keywords = new List<string>(),
                PublicationDate = newspaper.PublicationDate,
                ContentType = "Newspaper" // Tag for search filtering
            };

            // 5. Index the document
            await _elasticService.IndexDocumentAsync(searchDoc);

            return newspaper;
        }

        public async Task UpdateAsync(int id, NewspaperWriteDto request, CancellationToken cancellationToken)
        {
            var newspaper = await _newspaperRepository.GetByIdAsync(id, cancellationToken);
            if (newspaper == null) throw new Exception($"Newspaper with ID {id} not found.");

            // 6. Update SQL
            newspaper.Title = request.Title;
            newspaper.Description = request.Description;
            newspaper.Language = request.Language;
            newspaper.PublicationDate = request.PublicationDate;
            newspaper.Publisher = request.Publisher;
            newspaper.CategoryId = request.CategoryId;
            newspaper.Frequency = request.Frequency;
            newspaper.Price = request.Price;
            newspaper.PdfFilePath = request.PdfFilePath;

            _newspaperRepository.Update(newspaper);
            await _newspaperRepository.SaveChangesAsync(cancellationToken);

            // 7. Sync update to Elasticsearch
            var searchDoc = new SearchDocument
            {
                Id = $"Newspaper_{newspaper.Id}",
                OriginalId = newspaper.Id,
                Title = newspaper.Title,
                Description = newspaper.Description,
                Content = "",
                Author = "",
                Category = newspaper.CategoryId.ToString(),
                Publisher = newspaper.Publisher,
                Language = newspaper.Language,
                Keywords = new List<string>(),
                PublicationDate = newspaper.PublicationDate,
                ContentType = "Newspaper"
            };

            await _elasticService.IndexDocumentAsync(searchDoc);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var newspaper = await _newspaperRepository.GetByIdAsync(id, cancellationToken);
            if (newspaper != null)
            {
                // 8. Delete from SQL
                _newspaperRepository.Delete(newspaper);
                await _newspaperRepository.SaveChangesAsync(cancellationToken);

                // 9. Delete from Elasticsearch
                await _elasticService.DeleteDocumentAsync($"Newspaper_{id}");
            }
        }
    }
}