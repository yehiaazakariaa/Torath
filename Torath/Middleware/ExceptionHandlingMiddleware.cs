using System.Net;
using System.Text.Json;

namespace Torath.Middleware
{
    // A Middleware is a piece of code that intercepts every single request coming into your API.
    public class ExceptionHandlingMiddleware
    {
        // _next represents the next piece of middleware in the pipeline.
        private readonly RequestDelegate _next;

        // _logger allows us to write errors to the console without showing them to the user.
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        // The constructor grabs the _next delegate and the logger as soon as the app starts.
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // This method is triggered every single time a user makes an API call.
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // We tell the app to continue processing the user's request normally.
                await _next(context);
            }
            catch (Exception ex)
            {
                // IF the app crashes anywhere during the request, it gets caught right here.

                // 1. Secretly log the real error on the server so you (the dev) can fix it.
                _logger.LogError(ex, "An unexpected error occurred in the Torath API.");

                // 2. Send a safe, clean response back to the user so their screen doesn't break.
                await HandleExceptionAsync(context, ex);
            }
        }

        // This helper method builds the clean error message.
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // We tell the user's browser/frontend to expect a JSON response.
            context.Response.ContentType = "application/json";

            // We force the HTTP status code to be 500 (Internal Server Error).
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // We create a clean, safe object to return. 
            // We do NOT return the raw 'exception.StackTrace' because hackers could use that to see our code structure.
            var response = new
            {
                message = "An internal server error occurred. Please try again later.",
                detail = exception.Message
            };

            // Convert our C# object into JSON and send it back to the user.
            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}