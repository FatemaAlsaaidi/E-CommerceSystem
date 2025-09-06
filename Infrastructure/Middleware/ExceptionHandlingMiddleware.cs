using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // DbUpdateConcurrencyException
using Microsoft.Extensions.Logging;
using System.Text.Json;

public sealed class ExceptionHandlingMiddleware
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
            var traceId = context.TraceIdentifier;

            // Map exception -> status/title according to your codebase’s patterns
            var (status, title) = ex switch
            {
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency conflict"),
                ValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
                ArgumentNullException or ArgumentException
                                               => (StatusCodes.Status400BadRequest, "Bad request"),

                // FIRST: special-case DB messages
                InvalidOperationException ioe when ioe.Message.StartsWith("Database error:")
                                               => (StatusCodes.Status500InternalServerError, "Database error"),
                // THEN: general InvalidOperation
                InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation"),

                _ => (StatusCodes.Status500InternalServerError, "Server error")
            };


            _logger.LogError(ex,
                "{Title} ({Status}) on {Method} {Path} | TraceId={TraceId}",
                title, status, context.Request.Method, context.Request.Path, traceId);

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = ex.Message,          // consider hiding in Production
                Instance = context.Request.Path,
                Type = "about:blank"
            };
            problem.Extensions["traceId"] = traceId;

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = status;
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
