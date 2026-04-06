using System.Net;
using System.Text.Json;
using Backend.Exceptions;

namespace Backend.Middleware
{
    // ════════════════════════════════════════════════════════════════════════
    // GLOBAL EXCEPTION MIDDLEWARE
    // ════════════════════════════════════════════════════════════════════════
    // This sits at the very START of the request pipeline.
    // It wraps every single API request in a try-catch.
    //
    // WITHOUT this: any error returns a messy HTML error page.
    // WITH this:    every error returns a clean JSON response like:
    //               { "error": "Product not found", "statusCode": 404 }
    //
    // REGISTER in Program.cs (must be FIRST):
    //   app.UseMiddleware<GlobalExceptionMiddleware>();
    // ════════════════════════════════════════════════════════════════════════

    public class GlobalExceptionMiddleware
    {
        // _next = the next middleware in the pipeline (eventually your controller)
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        // This method is called for EVERY incoming HTTP request
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Pass request to the next middleware / controller
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                // 404: Resource not found
                _logger.LogWarning("Not found: {Message}", ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (Backend.Exceptions.ValidationException ex)
            {
                // 400: Bad request / business rule violation
                _logger.LogWarning("Validation error: {Message}", ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (UnauthorizedException ex)
            {
                // 401: Not authenticated
                _logger.LogWarning("Unauthorized: {Message}", ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (ForbiddenException ex)
            {
                // 403: Authenticated but not allowed
                _logger.LogWarning("Forbidden: {Message}", ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.Forbidden, ex.Message);
            }
            catch (ConflictException ex)
            {
                // 409: Duplicate / conflict
                _logger.LogWarning("Conflict: {Message}", ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.Conflict, ex.Message);
            }
            catch (AccountBannedException ex)
            {
                // 403: Banned user trying to login
                _logger.LogWarning("Banned account attempted login: {Message}", ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.Forbidden, ex.Message);
            }
            catch (Exception ex)
            {
                // 500: Unexpected / unknown error
                // Log the FULL exception with stack trace for debugging
                _logger.LogError(ex, "Unexpected error: {Message}", ex.Message);
                await WriteErrorResponse(
                    context,
                    HttpStatusCode.InternalServerError,
                    "An unexpected server error occurred. Please try again later."
                );
            }
        }

        // Helper: writes a clean JSON error response
        private static async Task WriteErrorResponse(
            HttpContext context,
            HttpStatusCode statusCode,
            string message)
        {
            context.Response.StatusCode  = (int)statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                statusCode = (int)statusCode,
                error      = message,
                timestamp  = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}