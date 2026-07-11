using System.ComponentModel.DataAnnotations;

namespace Torath.DTOs
{
    // --- BOOKS DTOs ---
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        // New Media Properties
        public string? CoverImageUrl { get; set; }
        public string? PdfFileUrl { get; set; }
    }

    public class BookWriteDto
    {
        [Required] public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required] public string Language { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        [Required] public int CategoryId { get; set; }

        // New Media Properties
        public string? CoverImageUrl { get; set; }
        public string? PdfFileUrl { get; set; }
    }

    // --- RESEARCH PAPERS DTOs ---
    public class ResearchPaperDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Abstract { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        // New Media Properties
        public string? CoverImageUrl { get; set; }
        public string? PdfFileUrl { get; set; }
    }

    public class ResearchPaperWriteDto
    {
        [Required] public string Title { get; set; } = string.Empty;
        public string Abstract { get; set; } = string.Empty;
        [Required] public string Author { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        [Required] public int CategoryId { get; set; }

        // New Media Properties
        public string? CoverImageUrl { get; set; }
        public string? PdfFileUrl { get; set; }
    }
}