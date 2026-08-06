using System;
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
        public double Rating { get; set; }
        public int ViewCount { get; set; }
        // New Media Properties
        public string? CoverImageUrl { get; set; }
        public string? PdfFileUrl { get; set; }
    }

    public class BookWriteDto
    {
        // Common Content Properties
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Language is required.")]
        [MaxLength(50)]
        public string Language { get; set; } = string.Empty;

        [Required]
        public DateTime PublicationDate { get; set; }

        [MaxLength(200)]
        public string Publisher { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? PdfFileUrl { get; set; }

        // Book-Specific Properties
        [Required(ErrorMessage = "ISBN is required.")]
        [MaxLength(20)]
        public string ISBN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Authors are required.")]
        [MaxLength(200)]
        public string Authors { get; set; } = string.Empty;

        public int NumberOfPages { get; set; }

        [MaxLength(50)]
        public string Edition { get; set; } = string.Empty;
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
        public double Rating { get; set; }
        public int ViewCount { get; set; }

        // New Media Properties
        public string? CoverImageUrl { get; set; }
        public string? PdfFileUrl { get; set; }
    }

    public class ResearchPaperWriteDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Abstract { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required.")]
        [MaxLength(200)]
        public string Author { get; set; } = string.Empty;

        [Required]
        public int PublicationYear { get; set; }

        [Required]
        public int CategoryId { get; set; }

        // New Media Properties
        public string? CoverImageUrl { get; set; }
        public string? PdfFileUrl { get; set; }
    }
} 