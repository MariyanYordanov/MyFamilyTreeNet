using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MyFamilyTreeNet.Api.Middleware
{
    public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, 
                "An unhandled exception occurred. RequestId: {RequestId}, Path: {Path}, User: {User}",
                System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier,
                context.Request.Path,
                context.User.Identity?.Name ?? "Anonymous");

            await HandleExceptionAsync(context, ex);
        }
    }
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            error = new
            {
                message = "An error occurred while processing your request.",
                requestId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier,
                timestamp = DateTime.UtcNow
            }
        };

        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentException:
                context.Response.StatusCode = 400; // Bad Request
                response = new
                {
                    error = new
                    {
                        message = "Invalid request parameters.",
                        requestId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier,
                        timestamp = DateTime.UtcNow
                    }
                };
                break;
                
            case UnauthorizedAccessException:
                context.Response.StatusCode = 401; // Unauthorized
                response = new
                {
                    error = new
                    {
                        message = "Unauthorized access.",
                        requestId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier,
                        timestamp = DateTime.UtcNow
                    }
                };
                break;
                
            case FileNotFoundException:
                context.Response.StatusCode = 404; // Not Found
                response = new
                {
                    error = new
                    {
                        message = "The requested resource was not found.",
                        requestId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier,
                        timestamp = DateTime.UtcNow
                    }
                };
                break;
                
            default:
                context.Response.StatusCode = 500; // Internal Server Error
                break;
        }

        // For API requests, return JSON response
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
        else
        {
            // For MVC requests, redirect to error page
            context.Response.Redirect($"/Error/{context.Response.StatusCode}");
        }
    }
}
}