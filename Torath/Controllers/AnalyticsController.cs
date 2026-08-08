using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Torath.Services;

namespace Torath.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Keep analytics locked down to admins only
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardAnalytics(CancellationToken cancellationToken = default)
        {
            var result = await _analyticsService.GetDashboardAnalyticsAsync(cancellationToken);
            return Ok(result);
        }
    }
}