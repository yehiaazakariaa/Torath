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
    public class BookService : IBookService
    {
        private readonly IRepository<Book> _bookRepository;

        // 2. Inject the Elasticsearch service
        private readonly IElasticSearchService _elasticService;

        public BookService(IRepository<Book> bookRepository, IElasticSearchService elasticService)
        {
            _bookRepository = bookRepository;
            _elasticService = elasticService;
        }

        public async Task<object> GetAllAsync(int page, int pageSize, string? category, string? language, CancellationToken cancellationToken)
        {
            // FIX: Added .Include(b => b.Category) so Entity Framework joins the table
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
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new { data, totalRecords, pageNumber = page, pageSize };
        }

        public async Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            // FIX: Swapped to GetQueryable so we can Include the Category before hitting the DB
            return await _bookRepository.GetQueryable()
                                        .Include(b => b.Category)
                                        .SingleOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<Book> CreateAsync(BookWriteDto request, CancellationToken cancellationToken)
        {
            // 3. Save the Book to SQL Server normally
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
                Edition = request.Edition
            };

            await _bookRepository.AddAsync(book, cancellationToken);
            await _bookRepository.SaveChangesAsync(cancellationToken);

            // 4. Map the newly created SQL record to the unified Elasticsearch Document
            // This explicitly fulfills the requirement to map Title, Description, Author, Category, etc.
            var searchDoc = new SearchDocument
            {
                Id = $"Book_{book.Id}", // Creates a unique ID across all content types
                OriginalId = book.Id,
                Title = book.Title,
                Description = book.Description,
                Content = "", // Books typically don't store full raw text in this DB model
                Author = book.Authors, // Maps the specific 'Authors' property to the unified 'Author' field
                Category = book.CategoryId.ToString(), // Storing Category ID. (Could also query the Category Name if preferred)
                Publisher = book.Publisher,
                Language = book.Language,
                Keywords = new List<string>(), // Empty for now, ready if you add tag features later
                PublicationDate = book.PublicationDate,
                ContentType = "Book" // Fulfills the requirement to track the Content Type
            };

            // 5. Send it to the Elasticsearch Index
            await _elasticService.IndexDocumentAsync(searchDoc);

            return book;
        }

        public async Task UpdateAsync(int id, BookWriteDto request, CancellationToken cancellationToken)
        {
            var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
            if (book == null) throw new Exception($"Book with ID {id} not found.");

            // 6. Update SQL Server normally
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

            _bookRepository.Update(book);
            await _bookRepository.SaveChangesAsync(cancellationToken);

            // 7. Sync the update to Elasticsearch. 
            // The IndexDocumentAsync method acts as an "Upsert" (Update or Insert), 
            // so sending the same Id ("Book_123") simply overwrites the old search data.
            var searchDoc = new SearchDocument
            {
                Id = $"Book_{book.Id}",
                OriginalId = book.Id,
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
                // 8. Delete from SQL Server
                _bookRepository.Delete(book);
                await _bookRepository.SaveChangesAsync(cancellationToken);

                // 9. Remove the corresponding document from Elasticsearch so it stops showing up in searches
                await _elasticService.DeleteDocumentAsync($"Book_{id}");
            }
        }
    }
}