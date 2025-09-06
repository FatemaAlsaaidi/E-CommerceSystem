using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;   // DbUpdateConcurrencyException
using Microsoft.Extensions.Logging;

namespace E_CommerceSystem.Middleware
{
    public sealed class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // map exception -> status/title
                var (status, title) = ex switch
                {
                    KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                    UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                    DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency conflict"),
                    ValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
                    ArgumentNullException or ArgumentException
                                                   => (StatusCodes.Status400BadRequest, "Bad request"),
                    InvalidOperationException ioe when ioe.Message.StartsWith("Database error:")
                                                   => (StatusCodes.Status500InternalServerError, "Database error"),
                    InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation"),
                    _ => (StatusCodes.Status500InternalServerError, "Server error")
                };

                var traceId = context.TraceIdentifier;
                _logger.LogError(ex,
                    "{Title} ({Status}) on {Method} {Path} | TraceId={TraceId}",
                    title, status, context.Request.Method,
                    context.Request.Path + context.Request.QueryString, traceId);

                var problem = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = ex.Message,
                    Instance = context.Request.Path + context.Request.QueryString
                };
                problem.Extensions["traceId"] = traceId;

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = status;
                await context.Response.WriteAsJsonAsync(problem);
            }
        }
    }

    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
            => app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
