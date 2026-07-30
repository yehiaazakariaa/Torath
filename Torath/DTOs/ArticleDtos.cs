namespace Torath.DTOs
{
    // This defines the exact JSON payload the user must send in POST and PUT requests
    public class ArticleWriteDto
    {
        // Required fields based on the project requirements
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public string Keywords { get; set; } = string.Empty;

        // Foreign Keys linking the article to its parent issue.
        // These are nullable (int?) because an article will belong to ONE of these, not both.
        public int? MagazineIssueId { get; set; }
        public int? NewspaperIssueId { get; set; }
    }
}