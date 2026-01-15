using System.Net;
using System.Text.Json;
using CompanyService.Business.Exceptions;

namespace CompanyService.Infrastructure.Middleware
{
    /// <summary>
    /// Global exception handling middleware
    /// Catches all unhandled exceptions and returns appropriate HTTP responses
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the GlobalExceptionHandlerMiddleware
        /// </summary>
        /// <param name="next">Next middleware in the pipeline</param>
        /// <param name="logger">Logger instance</param>
        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware to catch exceptions
        /// </summary>
        /// <param name="context">HTTP context</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handles exceptions and returns appropriate HTTP response
        /// Maps exception types to HTTP status codes
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="exception">Exception to handle</param>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            var result = string.Empty;

            // Map exception types to HTTP status codes
            switch (exception)
            {
                case NotFoundException notFoundException:
                    code = HttpStatusCode.NotFound;
                    result = JsonSerializer.Serialize(new { message = notFoundException.Message });
                    break;

                case ValidationException validationException:
                    code = HttpStatusCode.BadRequest;
                    result = JsonSerializer.Serialize(new 
                    { 
                        message = validationException.Message,
                        errors = validationException.Errors
                    });
                    break;

                case ArgumentException argumentException:
                    code = HttpStatusCode.BadRequest;
                    result = JsonSerializer.Serialize(new { message = argumentException.Message });
                    break;

                default:
                    // Generic error response for unhandled exceptions
                    result = JsonSerializer.Serialize(new { message = "An error occurred while processing your request." });
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(result);
        }
    }
}
