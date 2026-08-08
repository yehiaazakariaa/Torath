using System.Collections.Generic;

namespace Torath.DTOs
{
    public class AnalyticsDashboardDto
    {
        public int TotalViews { get; set; }
        public int TotalItems { get; set; }
        public List<AnalyticsItemDto> TopViewed { get; set; } = new List<AnalyticsItemDto>();
        public List<AnalyticsItemDto> TopRated { get; set; } = new List<AnalyticsItemDto>();
    }

    public class AnalyticsItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Book", "Magazine", etc.
        public int ViewCount { get; set; }
        public double Rating { get; set; }
    }
}