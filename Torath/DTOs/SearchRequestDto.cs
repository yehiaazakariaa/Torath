namespace Torath.DTOs
{
    public class SearchRequestDto
    {
        // The main search keyword (e.g., "Egypt", "Pyramids")
        public string? Query { get; set; }

        // Pagination (Requirement: Add support for pagination)
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Filtering (Requirements: Content Type, Category, Language, Publication Date)
        public string? ContentType { get; set; }
        public int? CategoryId { get; set; }
        public string? Language { get; set; }
        public DateTime? PublicationDateFrom { get; set; }
        public DateTime? PublicationDateTo { get; set; }

        // Sorting (Requirement: Add support for sorting)
        // E.g., "PublicationDate" or "Title". If left null, we default to relevance score.
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = true;
    }
}