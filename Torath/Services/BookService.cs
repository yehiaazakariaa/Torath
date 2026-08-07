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
    public class BookService : IBookService
    {
        private readonly IRepository<Book> _bookRepository;
        private readonly IElasticSearchService _elasticService;

        public BookService(IRepository<Book> bookRepository, IElasticSearchService elasticService)
        {
            _bookRepository = bookRepository;
            _elasticService = elasticService;
        }

        public async Task<object> GetAllAsync(int page, int pageSize, string? category, string? language, CancellationToken cancellationToken)
        {
            var query = _bookRepository.GetQueryable().Include(b => b.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(b => b.Category != null && b.Category.Name.Contains(category));
            }

            if (!string.IsNullOrWhiteSpace(language))
            {
                query = query.Where(b => b.Language.Contains(language));
            }

            var totalRecords = await query.CountAsync(cancellationToken);
            var data = await query
                .OrderByDescending(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    Language = b.Language,
                    Publisher = b.Publisher,
                    PublicationYear = b.PublicationDate.Year,
                    CategoryId = b.CategoryId,
                    CategoryName = b.Category != null ? b.Category.Name : string.Empty,
                    CoverImageUrl = b.CoverImageUrl,
                    PdfFileUrl = b.PdfFileUrl,
                    Rating = b.Rating,
                    ViewCount = b.ViewCount,
                    // ADDED FIELDS BELOW
                    ISBN = b.ISBN,
                    Authors = b.Authors,
                    NumberOfPages = b.NumberOfPages,
                    Edition = b.Edition
                })
                .ToListAsync(cancellationToken);

            return new { data, totalRecords, pageNumber = page, pageSize };
        }

        public async Task<BookDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var b = await _bookRepository.GetQueryable()
                                        .Include(b => b.Category)
                                        .SingleOrDefaultAsync(b => b.Id == id, cancellationToken);
            if (b == null) return null;

            return new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                Language = b.Language,
                Publisher = b.Publisher,
                PublicationYear = b.PublicationDate.Year,
                CategoryId = b.CategoryId,
                CategoryName = b.Category != null ? b.Category.Name : string.Empty,
                CoverImageUrl = b.CoverImageUrl,
                PdfFileUrl = b.PdfFileUrl,
                Rating = b.Rating,
                ViewCount = b.ViewCount,
                // ADDED FIELDS BELOW
                ISBN = b.ISBN,
                Authors = b.Authors,
                NumberOfPages = b.NumberOfPages,
                Edition = b.Edition
            };
        }

        public async Task<BookDto> CreateAsync(BookWriteDto request, CancellationToken cancellationToken)
        {
            var book = new Book
            {
                Title = request.Title,
                Description = request.Description,
                Language = request.Language,
                PublicationDate = request.PublicationDate,
                Publisher = request.Publisher,
                CategoryId = request.CategoryId,
                ISBN = request.ISBN,
                Authors = request.Authors,
                NumberOfPages = request.NumberOfPages,
                Edition = request.Edition,
                CoverImageUrl = request.CoverImageUrl,
                PdfFileUrl = request.PdfFileUrl
            };

            await _bookRepository.AddAsync(book, cancellationToken);
            await _bookRepository.SaveChangesAsync(cancellationToken);

            var searchDoc = new SearchDocument
            {
                Id = $"Book_{book.Id}",
                OriginalId = book.Id,
                DatabaseId = book.Id,
                Title = book.Title,
                Description = book.Description,
                Content = "",
                Author = book.Authors,
                Category = book.CategoryId.ToString(),
                Publisher = book.Publisher,
                Language = book.Language,
                Keywords = new List<string>(),
                PublicationDate = book.PublicationDate,
                ContentType = "Book"
            };

            await _elasticService.IndexDocumentAsync(searchDoc);

            return await GetByIdAsync(book.Id, cancellationToken) ?? throw new Exception("Failed to return created book.");
        }

        public async Task UpdateAsync(int id, BookWriteDto request, CancellationToken cancellationToken)
        {
            var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
            if (book == null) throw new Exception($"Book with ID {id} not found.");

            book.Title = request.Title;
            book.Description = request.Description;
            book.Language = request.Language;
            book.PublicationDate = request.PublicationDate;
            book.Publisher = request.Publisher;
            book.CategoryId = request.CategoryId;
            book.ISBN = request.ISBN;
            book.Authors = request.Authors;
            book.NumberOfPages = request.NumberOfPages;
            book.Edition = request.Edition;
            book.CoverImageUrl = request.CoverImageUrl;
            book.PdfFileUrl = request.PdfFileUrl;

            _bookRepository.Update(book);
            await _bookRepository.SaveChangesAsync(cancellationToken);

            var searchDoc = new SearchDocument
            {
                Id = $"Book_{book.Id}",
                OriginalId = book.Id,
                DatabaseId = book.Id,
                Title = book.Title,
                Description = book.Description,
                Content = "",
                Author = book.Authors,
                Category = book.CategoryId.ToString(),
                Publisher = book.Publisher,
                Language = book.Language,
                Keywords = new List<string>(),
                PublicationDate = book.PublicationDate,
                ContentType = "Book"
            };

            await _elasticService.IndexDocumentAsync(searchDoc);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
            if (book != null)
            {
                _bookRepository.Delete(book);
                await _bookRepository.SaveChangesAsync(cancellationToken);
                await _elasticService.DeleteDocumentAsync($"Book_{id}");
            }
        }
    }
}