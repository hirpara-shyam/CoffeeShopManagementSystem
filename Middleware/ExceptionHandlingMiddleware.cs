using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoffeeShopManagementSystem.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            var statusCode = (int)HttpStatusCode.InternalServerError;
            response.ContentType = "application/json";

            if (context.Request.Path.StartsWithSegments("/api"))
            {
                // Return JSON for API errors
                var result = JsonSerializer.Serialize(new { error = exception.Message, statusCode });
                response.StatusCode = statusCode;
                await response.WriteAsync(result);
            }
            else
            {
                // Redirect to error page for MVC
                context.Response.Redirect("/Home/Error");
            }
        }

    }
}
