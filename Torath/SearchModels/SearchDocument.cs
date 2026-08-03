using System;
using System.Collections.Generic;

namespace Torath.SearchModels
{
    // This model flattens all your different entities into one standard format for Elasticsearch.
    public class SearchDocument
    {
        // We need a unique ID for Elasticsearch. A good pattern is "ContentType_SQLId" (e.g., "Book_12")
        public string Id { get; set; } = string.Empty;

        // The SQL Database ID, in case we need to fetch the original record later
        public int OriginalId { get; set; }

        // Required fields mapped directly from your Section 6 guidelines
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty; // For articles/papers with actual text
        public string Author { get; set; } = string.Empty; // Or Authors
        public string Category { get; set; } = string.Empty; // The Category Name
        public string Publisher { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = new(); // List of tags/keywords
        public DateTime PublicationDate { get; set; }

        // This is crucial: it tells us if this document is a "Book", "Magazine", "Article", etc.
        public string ContentType { get; set; } = string.Empty;
    }
}