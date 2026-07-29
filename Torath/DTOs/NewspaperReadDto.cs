namespace Torath.DTOs
{
    public class NewspaperReadDto
    {
        public string Title { get; set; }
        public string Publisher { get; set; }
        public string Frequency { get; set; }
        public decimal Price { get; set; }
        public string Language { get; set; }
        public int CategoryId { get; set; } // Required to prevent the Foreign Key error!
        public string? PdfFilePath { get; set; }
    }
}