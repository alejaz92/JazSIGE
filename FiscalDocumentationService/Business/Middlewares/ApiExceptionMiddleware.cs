using FiscalDocumentationService.Business.Exceptions;
using System.Net;
using System.Text.Json;
using static FiscalDocumentationService.Business.Exceptions.FiscalDocumentationException;

namespace FiscalDocumentationService.Business.Middlewares
{
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public ApiExceptionMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (FiscalDocumentationException ex)
            {
                ctx.Response.ContentType = "application/json";

                ctx.Response.StatusCode = ex switch
                {
                    FiscalValidationException => (int)HttpStatusCode.BadRequest,
                    FiscalConfigurationException => (int)HttpStatusCode.Conflict,
                    ArcaDependencyException => (int)HttpStatusCode.BadGateway,
                    _ => (int)HttpStatusCode.InternalServerError
                };

                var payload = new
                {
                    error = ex.GetType().Name,
                    message = ex.Message
                };

                await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        }
    }
}
