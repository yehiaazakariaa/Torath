namespace Torath.Entities
{
    public class Article
    {
       
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public string Keywords { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public string? PdfFileUrl { get; set; }
        public double Rating { get; set; } = 0;
        public int ViewCount { get; set; } = 0;

        // --- Foreign Keys ---
        // Because an article might belong to a Magazine OR a Newspaper, we make these integers 
        // nullable (using the '?'). It will only ever have one of these filled out at a time.
        public int? MagazineIssueId { get; set; }
        public MagazineIssue? MagazineIssue { get; set; }

        public int? NewspaperIssueId { get; set; }
        public NewspaperIssue? NewspaperIssue { get; set; }
    }
}