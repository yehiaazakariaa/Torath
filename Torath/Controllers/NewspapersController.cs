using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using Torath.DTOs;
using Torath.Entities;
using Torath.Services;
using Torath.Repositories;

namespace Torath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewspapersController : ControllerBase
    {
        private readonly INewspaperService _newspaperService;
        private readonly TorathDbContext _context;

        public NewspapersController(INewspaperService newspaperService, TorathDbContext context)
        {
            _newspaperService = newspaperService;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _newspaperService.GetAllAsync(page, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
        {
            var newspaper = await _newspaperService.GetByIdAsync(id, cancellationToken);
            if (newspaper == null) return NotFound();
            return Ok(newspaper);
        }

        [HttpGet("{id}/issues")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetIssues(int id, CancellationToken cancellationToken = default)
        {
            var issues = await _newspaperService.GetIssuesByNewspaperIdAsync(id, cancellationToken);
            return Ok(issues);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] NewspaperWriteDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var createdNewspaper = await _newspaperService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdNewspaper.Id }, createdNewspaper);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] NewspaperWriteDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _newspaperService.UpdateAsync(id, request, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            await _newspaperService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id}/view")]
        [AllowAnonymous]
        public async Task<IActionResult> IncrementViewCount(int id, CancellationToken cancellationToken = default)
        {
            await _newspaperService.IncrementViewCountAsync(id, cancellationToken);
            return Ok();
        }

        [HttpPost("{id}/rate")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> RateNewspaper(int id, [FromBody] double rating, CancellationToken cancellationToken = default)
        {
            if (rating < 0 || rating > 5) return BadRequest("Rating must be between 0 and 5.");

            await _newspaperService.UpdateRatingAsync(id, rating, cancellationToken);
            return Ok();
        }

        [HttpPost("{id}/checkout")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> CreateCheckoutSession(int id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var newspaper = await _newspaperService.GetByIdAsync(id, cancellationToken);
            if (newspaper == null) return NotFound("Newspaper not found.");

            // Check if user already bought it
            var alreadyPurchased = await _context.UserPurchases
                .AnyAsync(p => p.UserId == userId && p.NewspaperId == id && p.IsPaymentComplete, cancellationToken);

            if (alreadyPurchased) return BadRequest("You already own this newspaper.");

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(newspaper.Price * 100), // Stripe expects cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = newspaper.Title
                                // Removed the Images array completely so empty URLs do not crash Stripe Checkout
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                // The URLs your frontend will handle after payment
                SuccessUrl = "http://localhost:3000/newspapers/" + id + "?success=true",
                CancelUrl = "http://localhost:3000/newspapers/" + id + "?canceled=true",
                ClientReferenceId = userId, // Pass the user ID so the webhook knows who bought it
                Metadata = new Dictionary<string, string>
                {
                    { "NewspaperId", id.ToString() }
                }
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            // Save pending purchase to database
            var purchase = new UserPurchase
            {
                UserId = userId,
                NewspaperId = id,
                PurchaseDate = DateTime.UtcNow,
                StripeSessionId = session.Id,
                IsPaymentComplete = false
            };
            await _context.UserPurchases.AddAsync(purchase, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { url = session.Url });
        }

        [HttpGet("{id}/download")]
        [Authorize(Roles = "User, Admin")]

        public async Task<IActionResult> DownloadNewspaper(int id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var newspaper = await _newspaperService.GetByIdAsync(id, cancellationToken);

            // Ensure this matches your database column name (PdfFilePath)
            if (newspaper == null || string.IsNullOrEmpty(newspaper.PdfFilePath))
                return NotFound("Newspaper file not found.");

            // Check if they own it (Admins can download anything without paying)
            var ownsNewspaper = await _context.UserPurchases
                .AnyAsync(p => p.UserId == userId && p.NewspaperId == id && p.IsPaymentComplete, cancellationToken);

            if (!ownsNewspaper && !isAdmin)
            {
                return Forbid("You must purchase this newspaper before downloading.");
            }

            // THE FIX: Parse the full URL to get just the filename
            var uri = new Uri(newspaper.PdfFilePath);
            var fileName = Path.GetFileName(uri.LocalPath);

            // Look for that filename inside your physical wwwroot/uploads/pdfs folder
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "pdfs", fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound($"File does not exist on server. Looked for: {filePath}");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath, cancellationToken);

            // Return the file for download
            return File(fileBytes, "application/pdf", $"{newspaper.Title}.pdf");
        }

        [HttpGet("admin/analytics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAnalytics(CancellationToken cancellationToken = default)
        {
            // 1. Calculate Total Downloads (Count of completed purchases)
            var totalDownloads = await _context.UserPurchases
                .CountAsync(p => p.IsPaymentComplete, cancellationToken);

            // 2. Calculate Total Revenue (Sum of the prices of all purchased newspapers)
            var totalRevenue = await _context.UserPurchases
                .Where(p => p.IsPaymentComplete)
                .Join(_context.Set<Newspaper>(),
                    purchase => purchase.NewspaperId,
                    newspaper => newspaper.Id,
                    (purchase, newspaper) => newspaper.Price)
                .SumAsync(cancellationToken);

            return Ok(new
            {
                TotalDownloads = totalDownloads,
                TotalRevenue = totalRevenue
            });
        }

        [HttpGet("{id}/ownership")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> CheckOwnership(int id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var ownsNewspaper = await _context.UserPurchases
                .AnyAsync(p => p.UserId == userId && p.NewspaperId == id && p.IsPaymentComplete, cancellationToken);

            return Ok(new { isOwned = ownsNewspaper });
        }

    }
}