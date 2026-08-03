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
    public class ArticleService : IArticleService
    {
        private readonly IRepository<Article> _articleRepository;

        // 2. Inject Elasticsearch Service
        private readonly IElasticSearchService _elasticService;

        public ArticleService(IRepository<Article> articleRepository, IElasticSearchService elasticService)
        {
            _articleRepository = articleRepository;
            _elasticService = elasticService;
        }

        public async Task<object> GetAllAsync(int page, int pageSize, string? author, CancellationToken cancellationToken)
        {
            var query = _articleRepository.GetQueryable();

            if (!string.IsNullOrWhiteSpace(author))
            {
                query = query.Where(a => a.Author.Contains(author));
            }

            var totalRecords = await query.CountAsync(cancellationToken);
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new { data, totalRecords, pageNumber = page, pageSize };
        }

        public async Task<Article> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _articleRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<Article> CreateAsync(ArticleWriteDto request, CancellationToken cancellationToken)
        {
            // 3. Save to SQL
            var article = new Article
            {
                Title = request.Title,
                Summary = request.Summary,
                Content = request.Content,
                Author = request.Author,
                PageNumber = request.PageNumber,
                Keywords = request.Keywords,
                MagazineIssueId = request.MagazineIssueId,
                NewspaperIssueId = request.NewspaperIssueId
            };

            await _articleRepository.AddAsync(article, cancellationToken);
            await _articleRepository.SaveChangesAsync(cancellationToken);

            // 4. Map to Elasticsearch Document
            var searchDoc = new SearchDocument
            {
                Id = $"Article_{article.Id}",
                OriginalId = article.Id,
                Title = article.Title,
                Description = article.Summary, // Maps Summary to Description
                Content = article.Content, // Articles actually have a Content body to map!
                Author = article.Author,
                Category = "", // Articles belong to issues, not categories directly in this model
                Publisher = "",
                Language = "",
                // Split comma-separated keywords string into a List<string> for Elasticsearch
                Keywords = string.IsNullOrWhiteSpace(article.Keywords)
                    ? new List<string>()
                    : article.Keywords.Split(',').Select(k => k.Trim()).ToList(),
                PublicationDate = DateTime.UtcNow, // Fallback if issues handle dates
                ContentType = "Article"
            };

            // 5. Index the document
            await _elasticService.IndexDocumentAsync(searchDoc);

            return article;
        }

        public async Task UpdateAsync(int id, ArticleWriteDto request, CancellationToken cancellationToken)
        {
            var article = await _articleRepository.GetByIdAsync(id, cancellationToken);
            if (article == null) throw new Exception($"Article with ID {id} not found.");

            // 6. Update SQL
            article.Title = request.Title;
            article.Summary = request.Summary;
            article.Content = request.Content;
            article.Author = request.Author;
            article.PageNumber = request.PageNumber;
            article.Keywords = request.Keywords;
            article.MagazineIssueId = request.MagazineIssueId;
            article.NewspaperIssueId = request.NewspaperIssueId;

            _articleRepository.Update(article);
            await _articleRepository.SaveChangesAsync(cancellationToken);

            // 7. Sync update to Elasticsearch
            var searchDoc = new SearchDocument
            {
                Id = $"Article_{article.Id}",
                OriginalId = article.Id,
                Title = article.Title,
                Description = article.Summary,
                Content = article.Content,
                Author = article.Author,
                Category = "",
                Publisher = "",
                Language = "",
                Keywords = string.IsNullOrWhiteSpace(article.Keywords)
                    ? new List<string>()
                    : article.Keywords.Split(',').Select(k => k.Trim()).ToList(),
                PublicationDate = DateTime.UtcNow,
                ContentType = "Article"
            };

            await _elasticService.IndexDocumentAsync(searchDoc);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var article = await _articleRepository.GetByIdAsync(id, cancellationToken);
            if (article != null)
            {
                // 8. Delete from SQL
                _articleRepository.Delete(article);
                await _articleRepository.SaveChangesAsync(cancellationToken);

                // 9. Delete from Elasticsearch
                await _elasticService.DeleteDocumentAsync($"Article_{id}");
            }
        }
    }
}