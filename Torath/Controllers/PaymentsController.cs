using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.IO;
using System.Threading.Tasks;
using Torath.Repositories; // Adjust for your DbContext
using Microsoft.EntityFrameworkCore;

namespace Torath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly TorathDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentsController(TorathDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var endpointSecret = _configuration["Stripe:WebhookSecret"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    endpointSecret
                );

                if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;

                    // Find the pending purchase and mark it complete
                    var purchase = await _context.UserPurchases
                        .FirstOrDefaultAsync(p => p.StripeSessionId == session.Id);

                    if (purchase != null)
                    {
                        purchase.IsPaymentComplete = true;
                        _context.UserPurchases.Update(purchase);
                        await _context.SaveChangesAsync();
                    }
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }
    }
}