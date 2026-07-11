using Microsoft.EntityFrameworkCore;
using Torath.DTOs;
using Torath.Entities;


namespace Torath.Services
{
    public class BookService : IBookService
    {
        private readonly TorathDbContext _context;
        private readonly IFileService _fileService;

        public BookService(TorathDbContext context, IFileService fileService) // Update constructor
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<PagedResponse<BookDto>> GetAllAsync(int page, int pageSize, string? category, string? language)
        {
            var query = _context.Books.Include(b => b.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(b => b.Category.Name.Contains(category));

            if (!string.IsNullOrWhiteSpace(language))
                query = query.Where(b => b.Language == language);

            var totalRecords = await query.CountAsync();
            var books = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    Language = b.Language,
                    Publisher = b.Publisher,
                    PublicationYear = b.PublicationYear,
                    CategoryId = b.CategoryId,
                    CategoryName = b.Category.Name,
                    CoverImageUrl = b.CoverImageUrl,
                    PdfFileUrl = b.PdfFileUrl // Added
                }).ToListAsync();

            return new PagedResponse<BookDto> { Data = books, TotalRecords = totalRecords, PageNumber = page, PageSize = pageSize };
        }

        public async Task<BookDto?> GetByIdAsync(int id)
        {
            var book = await _context.Books.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return null;

            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                Language = book.Language,
                Publisher = book.Publisher,
                PublicationYear = book.PublicationYear,
                CategoryId = book.CategoryId,
                CategoryName = book.Category.Name,
                CoverImageUrl = book.CoverImageUrl,
                PdfFileUrl = book.PdfFileUrl // Added
            };
        }

        public async Task<BookDto> CreateAsync(BookWriteDto request)
        {
            var book = new Book
            {
                Title = request.Title,
                Description = request.Description,
                Language = request.Language,
                Publisher = request.Publisher,
                PublicationYear = request.PublicationYear,
                CategoryId = request.CategoryId,
                CoverImageUrl = request.CoverImageUrl,
                PdfFileUrl = request.PdfFileUrl // Added
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(book.Id);
        }

        public async Task<bool> UpdateAsync(int id, BookWriteDto request)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;

            book.Title = request.Title; book.Description = request.Description; book.Language = request.Language;
            book.Publisher = request.Publisher; book.PublicationYear = request.PublicationYear; book.CategoryId = request.CategoryId;
            book.CoverImageUrl = request.CoverImageUrl; book.PdfFileUrl = request.PdfFileUrl; // Added

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;

            // Delete associated physical files before removing the database record
            _fileService.DeleteFile(book.CoverImageUrl);
            _fileService.DeleteFile(book.PdfFileUrl);

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}