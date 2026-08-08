using System.Threading;
using System.Threading.Tasks;
using Torath.DTOs;

namespace Torath.Services
{
    public interface IAnalyticsService
    {
        Task<AnalyticsDashboardDto> GetDashboardAnalyticsAsync(CancellationToken cancellationToken);
    }
}