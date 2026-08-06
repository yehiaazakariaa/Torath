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
            var query = _newspaperRepository.GetQueryable().Include(n => n.Category);

            var totalRecords = await query.CountAsync(cancellationToken);
            var data = await query
                .OrderByDescending(n => n.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NewspaperReadDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Publisher = n.Publisher,
                    Frequency = n.Frequency,
                    Price = n.Price,
                    Language = n.Language,
                    CategoryId = n.CategoryId,
                    PdfFilePath = n.PdfFilePath,
                    Rating = n.Rating,
                    ViewCount = n.ViewCount
                })
                .ToListAsync(cancellationToken);

            return new { data, totalRecords, pageNumber = page, pageSize };
        }

        public async Task<NewspaperReadDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var n = await _newspaperRepository.GetQueryable()
                .Include(nw => nw.Category)
                .FirstOrDefaultAsync(nw => nw.Id == id, cancellationToken);
            if (n == null) return null;

            return new NewspaperReadDto
            {
                Title = n.Title,
                Publisher = n.Publisher,
                Frequency = n.Frequency,
                Price = n.Price,
                Language = n.Language,
                CategoryId = n.CategoryId,
                PdfFilePath = n.PdfFilePath,
                Rating = n.Rating,
                ViewCount = n.ViewCount
            };
        }

        public async Task<IEnumerable<NewspaperIssueDto>> GetIssuesByNewspaperIdAsync(int newspaperId, CancellationToken cancellationToken)
        {
            return await _issueRepository.GetQueryable()
                .Where(i => i.NewspaperId == newspaperId)
                .Select(i => new NewspaperIssueDto
                {
                    Id = i.Id,
                    IssueNumber = i.IssueNumber,
                    PublicationDate = i.PublicationDate,
                    NewspaperId = i.NewspaperId,
                    Rating = i.Rating,
                    ViewCount = i.ViewCount
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<NewspaperReadDto> CreateAsync(NewspaperWriteDto request, CancellationToken cancellationToken)
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
                PdfFilePath = request.PdfFilePath,
                CoverImageUrl = request.CoverImageUrl
            };

            await _newspaperRepository.AddAsync(newspaper, cancellationToken);
            await _newspaperRepository.SaveChangesAsync(cancellationToken);

            var searchDoc = new SearchDocument
            {
                Id = $"Newspaper_{newspaper.Id}",
                OriginalId = newspaper.Id,
                DatabaseId = newspaper.Id,
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

            return await GetByIdAsync(newspaper.Id, cancellationToken) ?? throw new Exception("Failed to return created newspaper.");
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
            newspaper.CoverImageUrl = request.CoverImageUrl;

            _newspaperRepository.Update(newspaper);
            await _newspaperRepository.SaveChangesAsync(cancellationToken);

            var searchDoc = new SearchDocument
            {
                Id = $"Newspaper_{newspaper.Id}",
                OriginalId = newspaper.Id,
                DatabaseId = newspaper.Id,
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