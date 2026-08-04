using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Torath;
using Torath.Entities;
using Torath.SearchModels;
using Torath.Services;

namespace Torath.Controllers
{
    [Route("api/admin/search-index")]
    [ApiController]
    [Authorize(Roles = "Admin")] // RESTRICTED EXCLUSIVELY TO ADMINS
    public class AdminSearchController : ControllerBase
    {
        private readonly TorathDbContext _context;
        private readonly IElasticSearchService _elasticSearchService;

        public AdminSearchController(TorathDbContext context, IElasticSearchService elasticSearchService)
        {
            _context = context;
            _elasticSearchService = elasticSearchService;
        }

        [HttpPost("rebuild")]
        public async Task<IActionResult> RebuildAllIndexes()
        {
            await _elasticSearchService.CreateIndexIfNotExistsAsync();
            var allDocuments = new List<SearchDocument>();

            var books = await _context.Books.Select(b => new SearchDocument
            {
                Id = $"Book_{b.Id}",
                Title = b.Title,
                Description = b.Description,
                Content = "",
                Author = b.Authors,
                Language = b.Language,
                PublicationDate = b.PublicationDate,
                CategoryId = b.CategoryId,
                ContentType = "Book"
            }).ToListAsync();
            allDocuments.AddRange(books);

            var articles = await _context.Articles.Select(a => new SearchDocument
            {
                Id = $"Article_{a.Id}",
                Title = a.Title,
                Description = "",
                Content = "",
                Author = "",
                Language = "",
                PublicationDate = new System.DateTime(),
                CategoryId = null,
                ContentType = "Article"
            }).ToListAsync();
            allDocuments.AddRange(articles);

            var papers = await _context.ResearchPapers.Select(p => new SearchDocument
            {
                Id = $"ResearchPaper_{p.Id}",
                Title = p.Title,
                Description = p.Description,
                Content = "",
                Author = "",
                Language = "",
                PublicationDate = new System.DateTime(),
                CategoryId = null,
                ContentType = "ResearchPaper"
            }).ToListAsync();
            allDocuments.AddRange(papers);

            var magazines = await _context.Magazines.Select(m => new SearchDocument
            {
                Id = $"Magazine_{m.Id}",
                Title = m.Title,
                Description = "",
                Content = "",
                Author = "",
                Language = "",
                PublicationDate = new System.DateTime(),
                CategoryId = null,
                ContentType = "Magazine"
            }).ToListAsync();
            allDocuments.AddRange(magazines);

            var newspapers = await _context.Newspapers.Select(n => new SearchDocument
            {
                Id = $"Newspaper_{n.Id}",
                Title = n.Title,
                Description = "",
                Content = "",
                Author = "",
                Language = "",
                PublicationDate = new System.DateTime(),
                CategoryId = null,
                ContentType = "Newspaper"
            }).ToListAsync();
            allDocuments.AddRange(newspapers);

            await _elasticSearchService.BulkIndexDocumentsAsync(allDocuments);
            return Ok(new { Message = $"Successfully rebuilt index with {allDocuments.Count} total documents." });
        }

        [HttpPost("rebuild/books")]
        public async Task<IActionResult> RebuildBooksIndex()
        {
            await _elasticSearchService.CreateIndexIfNotExistsAsync();
            var books = await _context.Books.Select(b => new SearchDocument
            {
                Id = $"Book_{b.Id}",
                Title = b.Title,
                Description = b.Description,
                Content = "",
                Author = b.Authors,
                Language = b.Language,
                PublicationDate = b.PublicationDate,
                CategoryId = b.CategoryId,
                ContentType = "Book"
            }).ToListAsync();

            await _elasticSearchService.BulkIndexDocumentsAsync(books);
            return Ok(new { Message = $"Successfully rebuilt index for {books.Count} Books." });
        }

        [HttpPost("rebuild/articles")]
        public async Task<IActionResult> RebuildArticlesIndex()
        {
            await _elasticSearchService.CreateIndexIfNotExistsAsync();
            var articles = await _context.Articles.Select(a => new SearchDocument
            {
                Id = $"Article_{a.Id}",
                Title = a.Title,
                Description = "",
                Content = "",
                Author = "",
                Language = "",
                PublicationDate = new System.DateTime(),
                CategoryId = null,
                ContentType = "Article"
            }).ToListAsync();

            await _elasticSearchService.BulkIndexDocumentsAsync(articles);
            return Ok(new { Message = $"Successfully rebuilt index for {articles.Count} Articles." });
        }

        [HttpPost("rebuild/research-papers")]
        public async Task<IActionResult> RebuildResearchPapersIndex()
        {
            await _elasticSearchService.CreateIndexIfNotExistsAsync();
            var papers = await _context.ResearchPapers.Select(p => new SearchDocument
            {
                Id = $"ResearchPaper_{p.Id}",
                Title = p.Title,
                Description = p.Description,
                Content = "",
                Author = "",
                Language = "",
                PublicationDate = new System.DateTime(),
                CategoryId = null,
                ContentType = "ResearchPaper"
            }).ToListAsync();

            await _elasticSearchService.BulkIndexDocumentsAsync(papers);
            return Ok(new { Message = $"Successfully rebuilt index for {papers.Count} Research Papers." });
        }

        [HttpPost("rebuild/magazines")]
        public async Task<IActionResult> RebuildMagazinesIndex()
        {
            await _elasticSearchService.CreateIndexIfNotExistsAsync();
            var magazines = await _context.Magazines.Select(m => new SearchDocument
            {
                Id = $"Magazine_{m.Id}",
                Title = m.Title,
                Description = "",
                Content = "",
                Author = "",
                Language = "",
                PublicationDate = new System.DateTime(),
                CategoryId = null,
                ContentType = "Magazine"
            }).ToListAsync();

            await _elasticSearchService.BulkIndexDocumentsAsync(magazines);
            return Ok(new { Message = $"Successfully rebuilt index for {magazines.Count} Magazines." });
        }

        [HttpPost("rebuild/newspapers")]
        public async Task<IActionResult> RebuildNewspapersIndex()
        {
            await _elasticSearchService.CreateIndexIfNotExistsAsync();
            var newspapers = await _context.Newspapers.Select(n => new SearchDocument
            {
                Id = $"Newspaper_{n.Id}",
                Title = n.Title,
                Description = "",
                Content = "",
                Author = "",
                Language = "",
                PublicationDate = new System.DateTime(),
                CategoryId = null,
                ContentType = "Newspaper"
            }).ToListAsync();

            await _elasticSearchService.BulkIndexDocumentsAsync(newspapers);
            return Ok(new { Message = $"Successfully rebuilt index for {newspapers.Count} Newspapers." });
        }
    }
}