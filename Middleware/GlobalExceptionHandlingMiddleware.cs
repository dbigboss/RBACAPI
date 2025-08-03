using Microsoft.AspNetCore.Mvc;
using RBACApi.Exceptions;
using System.Net;
using System.Text.Json;

namespace RBACApi.Middleware;

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
        catch (Exception exception)
        {
            await LogExceptionAsync(context, exception);
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ProblemDetails();

        switch (exception)
        {
            case ValidationException validationException:
                response.Status = (int)HttpStatusCode.BadRequest;
                response.Title = "Validation Error";
                response.Detail = validationException.Message;
                response.Extensions.Add("errors", validationException.Errors);
                break;

            case NotFoundException notFoundException:
                response.Status = (int)HttpStatusCode.NotFound;
                response.Title = "Resource Not Found";
                response.Detail = notFoundException.Message;
                break;

            case UnauthorizedException unauthorizedException:
                response.Status = (int)HttpStatusCode.Unauthorized;
                response.Title = "Unauthorized Access";
                response.Detail = unauthorizedException.Message;
                break;

            case ForbiddenException forbiddenException:
                response.Status = (int)HttpStatusCode.Forbidden;
                response.Title = "Access Forbidden";
                response.Detail = forbiddenException.Message;
                break;

            case ConflictException conflictException:
                response.Status = (int)HttpStatusCode.Conflict;
                response.Title = "Resource Conflict";
                response.Detail = conflictException.Message;
                break;

            case BadRequestException badRequestException:
                response.Status = (int)HttpStatusCode.BadRequest;
                response.Title = "Bad Request";
                response.Detail = badRequestException.Message;
                break;

            case TimeoutException timeoutException:
                response.Status = (int)HttpStatusCode.RequestTimeout;
                response.Title = "Request Timeout";
                response.Detail = timeoutException.Message;
                break;

            case ArgumentException argumentException:
                response.Status = (int)HttpStatusCode.BadRequest;
                response.Title = "Invalid Argument";
                response.Detail = argumentException.Message;
                break;

            // case ArgumentNullException argumentNullException:
            //     response.Status = (int)HttpStatusCode.BadRequest;
            //     response.Title = "Missing Required Parameter";
            //     response.Detail = $"Required parameter '{argumentNullException.ParamName}' is missing or null";
            //     break;

            case InvalidOperationException invalidOperationException:
                response.Status = (int)HttpStatusCode.BadRequest;
                response.Title = "Invalid Operation";
                response.Detail = invalidOperationException.Message;
                break;

            case NotSupportedException notSupportedException:
                response.Status = (int)HttpStatusCode.NotImplemented;
                response.Title = "Operation Not Supported";
                response.Detail = notSupportedException.Message;
                break;

            case UnauthorizedAccessException unauthorizedAccessException:
                response.Status = (int)HttpStatusCode.Forbidden;
                response.Title = "Access Denied";
                response.Detail = unauthorizedAccessException.Message;
                break;

            default:
                response.Status = (int)HttpStatusCode.InternalServerError;
                response.Title = "Internal Server Error";
                response.Detail = "An unexpected error occurred. Please try again later.";
                break;
        }

        // Add common properties
        response.Type = $"https://httpstatuses.com/{response.Status}";
        response.Instance = context.Request.Path;

        // Add additional context for development environment
        if (IsDeploymentEnvironment(context) == "Development")
        {
            response.Extensions.Add("stackTrace", exception.StackTrace);
            response.Extensions.Add("source", exception.Source);
            response.Extensions.Add("innerException", exception.InnerException?.Message);
        }

        // Add correlation ID if available
        if (context.TraceIdentifier != null)
        {
            response.Extensions.Add("traceId", context.TraceIdentifier);
        }

        // Add timestamp
        response.Extensions.Add("timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

        context.Response.StatusCode = response.Status ?? (int)HttpStatusCode.InternalServerError;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private async Task LogExceptionAsync(HttpContext context, Exception exception)
    {
        var requestInfo = await GetRequestInfoAsync(context);
        var userInfo = GetUserInfo(context);
        
        // Create structured log properties
        var logProperties = new Dictionary<string, object>
        {
            ["TraceId"] = context.TraceIdentifier,
            ["RequestPath"] = context.Request.Path.Value ?? "Unknown",
            ["RequestMethod"] = context.Request.Method,
            ["RequestQuery"] = context.Request.QueryString.Value ?? "",
            ["UserAgent"] = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown",
            ["RemoteIP"] = GetClientIpAddress(context),
            ["UserId"] = userInfo.UserId,
            ["UserEmail"] = userInfo.UserEmail,
            ["UserRoles"] = userInfo.UserRoles,
            ["ExceptionType"] = exception.GetType().Name,
            ["RequestBody"] = requestInfo.RequestBody,
            ["RequestHeaders"] = requestInfo.Headers,
            ["Timestamp"] = DateTime.UtcNow
        };

        // Log with appropriate level based on exception type
        switch (exception)
        {
            case ValidationException validationEx:
                _logger.LogWarning(exception, 
                    "Validation error occurred for {UserId} at {RequestPath}: {Message}. Errors: {@ValidationErrors}",
                    userInfo.UserId ?? "Anonymous",
                    context.Request.Path,
                    exception.Message,
                    validationEx.Errors);
                break;

            case NotFoundException notFoundEx:
                _logger.LogWarning(exception,
                    "Resource not found for {UserId} at {RequestPath}: {ResourceType} with ID {ResourceId}",
                    userInfo.UserId ?? "Anonymous",
                    context.Request.Path,
                    notFoundEx.ResourceType ?? "Unknown",
                    notFoundEx.ResourceId ?? "Unknown");
                break;

            case UnauthorizedException:
                _logger.LogWarning(exception,
                    "Unauthorized access attempt from {RemoteIP} at {RequestPath}: {Message}",
                    GetClientIpAddress(context),
                    context.Request.Path,
                    exception.Message);
                break;

            case ForbiddenException forbiddenEx:
                _logger.LogWarning(exception,
                    "Forbidden access attempt by {UserId} at {RequestPath}: Attempted to {Action} {Resource}",
                    userInfo.UserId ?? "Anonymous",
                    context.Request.Path,
                    forbiddenEx.Action ?? "unknown action",
                    forbiddenEx.Resource ?? "unknown resource");
                break;

            case ConflictException conflictEx:
                _logger.LogWarning(exception,
                    "Resource conflict for {UserId} at {RequestPath}: {ResourceType} - {ConflictReason}",
                    userInfo.UserId ?? "Anonymous",
                    context.Request.Path,
                    conflictEx.ResourceType ?? "Unknown",
                    conflictEx.ConflictReason ?? exception.Message);
                break;

            case BadRequestException badReqEx:
                _logger.LogWarning(exception,
                    "Bad request from {UserId} at {RequestPath}: {Message}. Invalid field: {InvalidField}",
                    userInfo.UserId ?? "Anonymous",
                    context.Request.Path,
                    exception.Message,
                    badReqEx.InvalidField ?? "Unknown");
                break;

            case ArgumentException:
            case InvalidOperationException:
                _logger.LogWarning(exception,
                    "Client error for {UserId} at {RequestPath}: {ExceptionType} - {Message}",
                    userInfo.UserId ?? "Anonymous",
                    context.Request.Path,
                    exception.GetType().Name,
                    exception.Message);
                break;

            case TimeoutException:
                _logger.LogError(exception,
                    "Timeout occurred for {UserId} at {RequestPath}: {Message}",
                    userInfo.UserId ?? "Anonymous",
                    context.Request.Path,
                    exception.Message);
                break;

            default:
                _logger.LogError(exception,
                    "Unhandled exception occurred for {UserId} at {RequestPath}: {ExceptionType} - {Message}. {@LogProperties}",
                    userInfo.UserId ?? "Anonymous",
                    context.Request.Path,
                    exception.GetType().Name,
                    exception.Message,
                    logProperties);
                break;
        }

        // Additional critical error logging for system exceptions
        if (!(exception is BaseException))
        {
            _logger.LogCritical(exception,
                "CRITICAL: System exception occurred. TraceId: {TraceId}, Path: {Path}, User: {UserId}, Exception: {ExceptionType}",
                context.TraceIdentifier,
                context.Request.Path,
                userInfo.UserId ?? "Anonymous",
                exception.GetType().FullName);
        }
    }

    private static async Task<RequestInfo> GetRequestInfoAsync(HttpContext context)
    {
        var requestInfo = new RequestInfo();

        // Capture request headers (exclude sensitive ones)
        var safeHeaders = new Dictionary<string, string>();
        foreach (var header in context.Request.Headers.Where(h => !IsSensitiveHeader(h.Key)))
        {
            safeHeaders[header.Key] = header.Value.ToString();
        }
        requestInfo.Headers = safeHeaders;

        // Capture request body for POST/PUT requests (with size limit)
        if (context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                context.Request.EnableBuffering();
                context.Request.Body.Position = 0;
                
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                // Limit body size for logging and mask sensitive data
                if (body.Length > 1000)
                {
                    body = body.Substring(0, 1000) + "... (truncated)";
                }

                requestInfo.RequestBody = MaskSensitiveData(body);
            }
            catch
            {
                requestInfo.RequestBody = "[Unable to read request body]";
            }
        }

        return requestInfo;
    }

    private static UserInfo GetUserInfo(HttpContext context)
    {
        var userInfo = new UserInfo();

        if (context.User?.Identity?.IsAuthenticated == true)
        {
            userInfo.UserId = context.User.FindFirst("sub")?.Value ?? 
                             context.User.FindFirst("id")?.Value ?? 
                             context.User.FindFirst("userId")?.Value ??
                             context.User.Identity.Name;

            userInfo.UserEmail = context.User.FindFirst("email")?.Value ?? 
                                context.User.FindFirst("EmailAddress")?.Value;

            userInfo.UserRoles = context.User.Claims
                .Where(c => c.Type == "role" || c.Type.EndsWith("/role"))
                .Select(c => c.Value)
                .ToList();
        }

        return userInfo;
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP first (in case of load balancer/proxy)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private static bool IsSensitiveHeader(string headerName)
    {
        var sensitiveHeaders = new[]
        {
            "Authorization", "Cookie", "X-API-Key", "X-Auth-Token", 
            "Authentication", "Proxy-Authorization"
        };

        return sensitiveHeaders.Any(h => 
            string.Equals(h, headerName, StringComparison.OrdinalIgnoreCase));
    }

    private static string MaskSensitiveData(string data)
    {
        if (string.IsNullOrEmpty(data)) return data;

        // Mask common sensitive fields in JSON
        var sensitivePatterns = new Dictionary<string, string>
        {
            { @"""[Pp]assword""\s*:\s*""[^""]*""", @"""password"":""***""" },
            { @"""[Tt]oken""\s*:\s*""[^""]*""", @"""token"":""***""" },
            { @"""[Aa]pi[Kk]ey""\s*:\s*""[^""]*""", @"""apiKey"":""***""" },
            { @"""[Ss]ecret""\s*:\s*""[^""]*""", @"""secret"":""***""" }
        };

        var maskedData = data;
        foreach (var pattern in sensitivePatterns)
        {
            maskedData = System.Text.RegularExpressions.Regex.Replace(
                maskedData, pattern.Key, pattern.Value, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return maskedData;
    }

    private static string IsDeploymentEnvironment(HttpContext context)
    {
        var environment = context.RequestServices.GetService<IWebHostEnvironment>();
        return environment?.EnvironmentName ?? "Production";
    }

    private class RequestInfo
    {
        public Dictionary<string, string> Headers { get; set; } = new();
        public string RequestBody { get; set; } = string.Empty;
    }

    private class UserInfo
    {
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public List<string> UserRoles { get; set; } = new();
    }
}