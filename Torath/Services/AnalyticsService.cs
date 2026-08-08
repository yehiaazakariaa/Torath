using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Torath.DTOs;
using Torath.Repositories; // Adjust if your DbContext namespace is different

namespace Torath.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly TorathDbContext _context;

        public AnalyticsService(TorathDbContext context)
        {
            _context = context;
        }

        public async Task<AnalyticsDashboardDto> GetDashboardAnalyticsAsync(CancellationToken cancellationToken)
        {
            // 1. Calculate global totals via SQL SUM/COUNT
            var totalBooks = await _context.Books.CountAsync(cancellationToken);
            var totalMags = await _context.Magazines.CountAsync(cancellationToken);
            var totalNews = await _context.Newspapers.CountAsync(cancellationToken);
            var totalArticles = await _context.Articles.CountAsync(cancellationToken);
            var totalPapers = await _context.ResearchPapers.CountAsync(cancellationToken);

            var viewBooks = await _context.Books.SumAsync(x => x.ViewCount, cancellationToken);
            var viewMags = await _context.Magazines.SumAsync(x => x.ViewCount, cancellationToken);
            var viewNews = await _context.Newspapers.SumAsync(x => x.ViewCount, cancellationToken);
            var viewArticles = await _context.Articles.SumAsync(x => x.ViewCount, cancellationToken);
            var viewPapers = await _context.ResearchPapers.SumAsync(x => x.ViewCount, cancellationToken);

            var allItems = new List<AnalyticsItemDto>();

            // 2. Fetch Top 10 Viewed from EACH table (Database does the sorting!)
            allItems.AddRange(await _context.Books.OrderByDescending(x => x.ViewCount).Take(10)
                .Select(x => new AnalyticsItemDto { Id = x.Id, Title = x.Title, Type = "Book", ViewCount = x.ViewCount, Rating = x.Rating }).ToListAsync(cancellationToken));

            allItems.AddRange(await _context.Magazines.OrderByDescending(x => x.ViewCount).Take(10)
                .Select(x => new AnalyticsItemDto { Id = x.Id, Title = x.Title, Type = "Magazine", ViewCount = x.ViewCount, Rating = x.Rating }).ToListAsync(cancellationToken));

            allItems.AddRange(await _context.Newspapers.OrderByDescending(x => x.ViewCount).Take(10)
                .Select(x => new AnalyticsItemDto { Id = x.Id, Title = x.Title, Type = "Newspaper", ViewCount = x.ViewCount, Rating = x.Rating }).ToListAsync(cancellationToken));

            allItems.AddRange(await _context.Articles.OrderByDescending(x => x.ViewCount).Take(10)
                .Select(x => new AnalyticsItemDto { Id = x.Id, Title = x.Title, Type = "Article", ViewCount = x.ViewCount, Rating = x.Rating }).ToListAsync(cancellationToken));

            allItems.AddRange(await _context.ResearchPapers.OrderByDescending(x => x.ViewCount).Take(10)
                .Select(x => new AnalyticsItemDto { Id = x.Id, Title = x.Title, Type = "Research Paper", ViewCount = x.ViewCount, Rating = x.Rating }).ToListAsync(cancellationToken));

            // 3. Compile the final Top 10 across all categories
            var topViewed = allItems.OrderByDescending(x => x.ViewCount).Take(10).ToList();
            var topRated = allItems.OrderByDescending(x => x.Rating).Take(10).ToList();

            return new AnalyticsDashboardDto
            {
                TotalItems = totalBooks + totalMags + totalNews + totalArticles + totalPapers,
                TotalViews = viewBooks + viewMags + viewNews + viewArticles + viewPapers,
                TopViewed = topViewed,
                TopRated = topRated
            };
        }
    }
}