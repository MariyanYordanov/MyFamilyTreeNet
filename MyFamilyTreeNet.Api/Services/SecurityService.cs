using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyFamilyTreeNet.Api.Security
{
    public class SecurityService : ISecurityService
    {
        private readonly ILogger<SecurityService> _logger;

        public SecurityService(ILogger<SecurityService> logger)
        {
            _logger = logger;
        }

        public string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remove script tags
            input = System.Text.RegularExpressions.Regex.Replace(input, 
                @"<script[^>]*>.*?</script>", "", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | 
                System.Text.RegularExpressions.RegexOptions.Singleline);

            // Remove javascript: URLs
            input = System.Text.RegularExpressions.Regex.Replace(input, 
                @"javascript:", "", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Remove event handlers (onclick, onload, etc.)
            input = System.Text.RegularExpressions.Regex.Replace(input, 
                @"on\w+\s*=", "", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Remove potentially dangerous HTML tags
            var dangerousTags = new[] { "script", "object", "embed", "link", "meta", "iframe", "frame", "frameset" };
            foreach (var tag in dangerousTags)
            {
                input = System.Text.RegularExpressions.Regex.Replace(input, 
                    $@"<{tag}[^>]*>.*?</{tag}>", "", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | 
                    System.Text.RegularExpressions.RegexOptions.Singleline);
                
                input = System.Text.RegularExpressions.Regex.Replace(input, 
                    $@"<{tag}[^>]*/>", "", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            return input.Trim();
        }

        public bool IsValidInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            // Check for script tags
            if (System.Text.RegularExpressions.Regex.IsMatch(input, 
                @"<script[^>]*>", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                return false;

            // Check for javascript: URLs
            if (System.Text.RegularExpressions.Regex.IsMatch(input, 
                @"javascript:", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                return false;

            // Check for event handlers
            if (System.Text.RegularExpressions.Regex.IsMatch(input, 
                @"on\w+\s*=", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                return false;

            return true;
        }

        public bool ContainsHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(input, @"<[^>]*>");
        }

        public string EscapeHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return System.Net.WebUtility.HtmlEncode(input);
        }

        public bool ValidateCSRFToken(HttpContext context)
        {
            try
            {
                var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
                antiforgery.ValidateRequestAsync(context).Wait();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CSRF token validation failed for request {Path}", context.Request.Path);
                return false;
            }
        }
    }

    /// <summary>
    /// Custom authorize attribute with enhanced security logging
    /// </summary>
    public class SecureAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<SecureAuthorizeAttribute>>();
            
            // Log authorization attempt
            logger.LogInformation(
                "Authorization attempt for {Path} by user {User} with roles {Roles}",
                context.HttpContext.Request.Path,
                context.HttpContext.User.Identity?.Name ?? "Anonymous",
                string.Join(", ", context.HttpContext.User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)));

            // Check if user is authenticated
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Log authorization result
            if (context.Result is ForbidResult)
            {
                logger.LogWarning(
                    "Access denied to {Path} for user {User}",
                    context.HttpContext.Request.Path,
                    context.HttpContext.User.Identity?.Name ?? "Anonymous");
            }
        }
    }

    /// <summary>
    /// Rate limiting attribute to prevent abuse
    /// </summary>
    public class RateLimitAttribute : ActionFilterAttribute
    {
        private readonly int _requests;
        private readonly int _minutes;
        private static readonly Dictionary<string, List<DateTime>> _requests_log = new();

        public RateLimitAttribute(int requests = 60, int minutes = 1)
        {
            _requests = requests;
            _minutes = minutes;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var key = GetClientKey(context.HttpContext);
            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-_minutes);

            lock (_requests_log)
            {
                if (!_requests_log.ContainsKey(key))
                    _requests_log[key] = new List<DateTime>();

                // Remove old requests outside the time window
                _requests_log[key] = _requests_log[key].Where(time => time > windowStart).ToList();

                if (_requests_log[key].Count >= _requests)
                {
                    context.Result = new ContentResult
                    {
                        Content = "Rate limit exceeded. Please try again later.",
                        StatusCode = 429
                    };

                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RateLimitAttribute>>();
                    logger.LogWarning(
                        "Rate limit exceeded for {Key} on {Path}. {Count} requests in {Minutes} minutes.",
                        key, context.HttpContext.Request.Path, _requests_log[key].Count, _minutes);

                    return;
                }

                _requests_log[key].Add(now);
            }

            base.OnActionExecuting(context);
        }

        private string GetClientKey(HttpContext context)
        {
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var user = context.User.Identity?.Name ?? "anonymous";
            
            return $"{ip}:{user}:{userAgent.GetHashCode()}";
        }
    }

    /// <summary>
    /// Input validation filter
    /// </summary>
    public class ValidateInputAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var securityService = context.HttpContext.RequestServices.GetRequiredService<ISecurityService>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ValidateInputAttribute>>();

            foreach (var parameter in context.ActionArguments)
            {
                if (parameter.Value is string stringValue)
                {
                    if (!securityService.IsValidInput(stringValue))
                    {
                        logger.LogWarning(
                            "Invalid input detected in parameter {Parameter} for action {Action}. Value: {Value}",
                            parameter.Key, context.ActionDescriptor.DisplayName, stringValue);

                        context.Result = new BadRequestObjectResult(new
                        {
                            error = "Invalid input detected",
                            parameter = parameter.Key
                        });
                        return;
                    }
                }
                else if (parameter.Value != null)
                {
                    // Validate properties of complex objects
                    ValidateObjectProperties(parameter.Value, securityService, logger, context);
                    if (context.Result != null) return;
                }
            }

            base.OnActionExecuting(context);
        }

        private void ValidateObjectProperties(object obj, ISecurityService securityService, 
            ILogger logger, ActionExecutingContext context)
        {
            var properties = obj.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(string) && p.CanRead);

            foreach (var property in properties)
            {
                var value = property.GetValue(obj) as string;
                if (!string.IsNullOrEmpty(value) && !securityService.IsValidInput(value))
                {
                    logger.LogWarning(
                        "Invalid input detected in property {Property} of {Type}. Value: {Value}",
                        property.Name, obj.GetType().Name, value);

                    context.Result = new BadRequestObjectResult(new
                    {
                        error = "Invalid input detected",
                        property = property.Name
                    });
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Content Security Policy helper
    /// </summary>
    public static class ContentSecurityPolicy
    {
        public static string GetPolicyString()
        {
            return string.Join("; ", new[]
            {
                "default-src 'self'",
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://d3js.org",
                "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com",
                "img-src 'self' data: https:",
                "font-src 'self' https://cdnjs.cloudflare.com",
                "connect-src 'self'",
                "media-src 'self'",
                "object-src 'none'",
                "frame-src 'none'",
                "base-uri 'self'",
                "form-action 'self'",
                "frame-ancestors 'none'",
                "upgrade-insecure-requests"
            });
        }
    }
}