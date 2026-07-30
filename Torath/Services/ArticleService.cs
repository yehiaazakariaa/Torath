using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Torath.Entities;
using Torath.DTOs;

namespace Torath.Services
{
    public class ArticleService : IArticleService
    {
        private readonly TorathDbContext _context;

        public ArticleService(TorathDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetAllAsync(int page, int pageSize, string? author)
        {
            // 1. Start with the base query
            var query = _context.Articles.AsQueryable();

            // 2. Apply filtering if the 'author' parameter was provided[cite: 1]
            if (!string.IsNullOrWhiteSpace(author))
            {
                // Uses .Contains to allow partial matches (e.g., searching "John" finds "John Doe")
                query = query.Where(a => a.Author.Contains(author));
            }

            // 3. Count total records after filtering for accurate pagination metadata
            var totalRecords = await query.CountAsync();

            // 4. Apply pagination (Skip and Take)
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 5. Return the expected anonymous object structure
            return new
            {
                data = data,
                totalRecords = totalRecords,
                pageNumber = page,
                pageSize = pageSize
            };
        }

        public async Task<Article> GetByIdAsync(int id)
        {
            return await _context.Articles.FindAsync(id);
        }

        public async Task<Article> CreateAsync(ArticleWriteDto request)
        {
            // Optional but recommended: Ensure the article is assigned to exactly one parent type
            if (request.MagazineIssueId == null && request.NewspaperIssueId == null)
            {
                throw new Exception("An article must be assigned to either a MagazineIssueId or a NewspaperIssueId.");
            }

            // Map DTO to Entity[cite: 1]
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

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return article;
        }

        public async Task UpdateAsync(int id, ArticleWriteDto request)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                throw new Exception($"Article with ID {id} not found.");
            }

            // Apply updates
            article.Title = request.Title;
            article.Summary = request.Summary;
            article.Content = request.Content;
            article.Author = request.Author;
            article.PageNumber = request.PageNumber;
            article.Keywords = request.Keywords;
            article.MagazineIssueId = request.MagazineIssueId;
            article.NewspaperIssueId = request.NewspaperIssueId;

            _context.Articles.Update(article);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article != null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
            }
        }
    }
}