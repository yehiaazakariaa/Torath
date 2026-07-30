using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Torath.Entities;
using Torath.DTOs;
using Torath.Repositories; // Add this using statement

namespace Torath.Services
{
    public class ArticleService : IArticleService
    {
        // Inject the Repository instead of the DbContext!
        private readonly IRepository<Article> _articleRepository;

        public ArticleService(IRepository<Article> articleRepository)
        {
            _articleRepository = articleRepository;
        }

        public async Task<object> GetAllAsync(int page, int pageSize, string? author, CancellationToken cancellationToken)
        {
            var query = _articleRepository.GetQueryable(); // Use the repository's queryable

            if (!string.IsNullOrWhiteSpace(author))
            {
                query = query.Where(a => a.Author.Contains(author));
            }

            // Pass the token to EF Core methods
            var totalRecords = await query.CountAsync(cancellationToken);
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new { data, totalRecords, pageNumber = page, pageSize };
        }

        public async Task<Article> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            // Pass the token down
            return await _articleRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<Article> CreateAsync(ArticleWriteDto request, CancellationToken cancellationToken)
        {
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

            // Use repository methods
            await _articleRepository.AddAsync(article, cancellationToken);
            await _articleRepository.SaveChangesAsync(cancellationToken);

            return article;
        }

        public async Task UpdateAsync(int id, ArticleWriteDto request, CancellationToken cancellationToken)
        {
            var article = await _articleRepository.GetByIdAsync(id, cancellationToken);
            if (article == null) throw new Exception($"Article with ID {id} not found.");

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
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var article = await _articleRepository.GetByIdAsync(id, cancellationToken);
            if (article != null)
            {
                _articleRepository.Delete(article);
                await _articleRepository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}