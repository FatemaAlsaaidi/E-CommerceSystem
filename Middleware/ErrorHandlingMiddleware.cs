using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // DbUpdateConcurrencyException

namespace E_CommerceSystem.Middleware
{
    // ????? "conventional middleware": ???? RequestDelegate ?? ??? ctor
    public sealed class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // ????? ????????? -> ??? ?????? + ?????
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

                // ??? ?????
                _logger.LogError(ex,
                    "{Title} ({Status}) on {Method} {Path} | TraceId={TraceId}",
                    title, status, context.Request.Method,
                    context.Request.Path + context.Request.QueryString, traceId);

                // RFC-7807 ProblemDetails
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

                // ???? JsonSerializer — ???? ASP.NET ???? JSON
                await context.Response.WriteAsJsonAsync(problem);
            }
        }
    }

    public static class ErrorHandlingExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
            => app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
