using Microsoft.EntityFrameworkCore;
using Torath.Middleware;
namespace Torath
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // --- PHASE 1: SETUP (Gathering the tools) ---

            // This creates the builder that will construct our web application.
            var builder = WebApplication.CreateBuilder(args);

            // Tells the app that we are using API Controllers to handle web requests.
            builder.Services.AddControllers();
            // This tells Torath to use SQL Server and grabs the location from appsettings.json
            builder.Services.AddDbContext<TorathDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // -------------------------------------

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // These two lines set up the Swagger UI so we can visually test our API endpoints.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // (Later, we will add Entity Framework and JWT setup here)

            // Once all our tools are gathered, we build the actual application.
            var app = builder.Build();


            // --- PHASE 2: THE PIPELINE (How a request travels) ---
            // ORDER MATTERS HERE! Requests flow from top to bottom.

            // If we are coding locally on our machine (Development), turn on the visual Swagger page.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // 1. THE SAFETY NET: We plug in our custom error handler FIRST. 
            // Because it is at the top, it wraps around everything below it and catches any crashes.
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // 2. SECURITY: Automatically redirects standard HTTP traffic to secure HTTPS.
            app.UseHttpsRedirection();

            // 3. THE BOUNCER: Checks the user's JWT token and asks, "Who are you?" (We configure this later).
            app.UseAuthentication();

            // 4. THE MANAGER: Looks at the user's role and asks, "Are you an Admin or User? Are you allowed to do this?"
            app.UseAuthorization();

            // 5. THE ROUTER: Looks at the URL (like /api/books) and fires the correct C# code in our Controllers.
            app.MapControllers();

            // Finally, start the server and listen for incoming requests!
            app.Run();
        }
    }
}