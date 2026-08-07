namespace Torath.DTOs
{
    public class NewspaperReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public decimal Price { get; set; }
      
        public double Rating { get; set; }
        public int ViewCount { get; set; }
        public int CategoryId { get; set; } // Required to prevent the Foreign Key error!
        public string? PdfFilePath { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
    }
}