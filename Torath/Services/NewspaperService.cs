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
    public class NewspaperService : INewspaperService
    {
        private readonly IRepository<Newspaper> _newspaperRepository;
        private readonly IRepository<NewspaperIssue> _issueRepository;
        private readonly IElasticSearchService _elasticService;

        public NewspaperService(IRepository<Newspaper> newspaperRepository, IRepository<NewspaperIssue> issueRepository, IElasticSearchService elasticService)
        {
            _newspaperRepository = newspaperRepository;
            _issueRepository = issueRepository;
            _elasticService = elasticService;
        }

        public async Task<object> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            // Included Category to prevent null reference on the frontend
            var query = _newspaperRepository.GetQueryable().Include(n => n.Category);

            var totalRecords = await query.CountAsync(cancellationToken);
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new { data, totalRecords, pageNumber = page, pageSize };
        }

        public async Task<Newspaper?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _newspaperRepository.GetQueryable()
                .Include(n => n.Category)
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<NewspaperIssue>> GetIssuesByNewspaperIdAsync(int newspaperId, CancellationToken cancellationToken)
        {
            return await _issueRepository.GetQueryable()
                .Where(i => i.NewspaperId == newspaperId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Newspaper> CreateAsync(NewspaperWriteDto request, CancellationToken cancellationToken)
        {
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

            var searchDoc = new SearchDocument
            {
                Id = $"Newspaper_{newspaper.Id}",
                OriginalId = newspaper.Id,
                DatabaseId = newspaper.Id, // Mapped for frontend
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

            return newspaper;
        }

        public async Task UpdateAsync(int id, NewspaperWriteDto request, CancellationToken cancellationToken)
        {
            var newspaper = await _newspaperRepository.GetByIdAsync(id, cancellationToken);
            if (newspaper == null) throw new Exception($"Newspaper with ID {id} not found.");

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

            var searchDoc = new SearchDocument
            {
                Id = $"Newspaper_{newspaper.Id}",
                OriginalId = newspaper.Id,
                DatabaseId = newspaper.Id, // Mapped for frontend
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
                _newspaperRepository.Delete(newspaper);
                await _newspaperRepository.SaveChangesAsync(cancellationToken);
                await _elasticService.DeleteDocumentAsync($"Newspaper_{id}");
            }
        }
    }
}