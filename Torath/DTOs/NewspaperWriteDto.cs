using System;

namespace Torath.DTOs
{
    public class NewspaperWriteDto
    {
        // Common Content Information[cite: 1]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; }
        public string Publisher { get; set; } = string.Empty;
        public int CategoryId { get; set; }

        // Newspaper-Specific Information
        public string Frequency { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PdfFilePath { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
    }
}