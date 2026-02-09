using System.Net;
using System.Text.Json;

namespace UserManagementAPI.Middleware;

/// <summary>
/// Middleware for handling unhandled exceptions and returning consistent error responses
/// Ensures all errors are caught and returned in standardized JSON format
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes the ErrorHandlingMiddleware
    /// </summary>
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware to handle exceptions
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
            await HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// Handles exceptions and returns standardized error response
    /// </summary>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        // Determine status code and message based on exception type
        var (statusCode, message) = GetStatusCodeAndMessage(exception);
        response.StatusCode = statusCode;

        // Create standardized error response object
        var errorResponse = new
        {
            error = new
            {
                message = message,
                exceptionType = exception.GetType().Name,
                timestamp = DateTime.UtcNow,
                statusCode = statusCode,
                path = context.Request.Path,
                method = context.Request.Method
            }
        };

        // Include stack trace in development environment only
        if (!string.IsNullOrEmpty(exception.StackTrace))
        {
            var dev = new
            {
                error = new
                {
                    message = message,
                    exceptionType = exception.GetType().Name,
                    timestamp = DateTime.UtcNow,
                    statusCode = statusCode,
                    path = context.Request.Path,
                    method = context.Request.Method,
                    stackTrace = exception.StackTrace,
                    innerException = exception.InnerException?.Message
                }
            };

            return context.Response.WriteAsJsonAsync(dev);
        }

        return context.Response.WriteAsJsonAsync(errorResponse);
    }

    /// <summary>
    /// Determines appropriate HTTP status code and error message based on exception type
    /// </summary>
    private static (int statusCode, string message) GetStatusCodeAndMessage(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => (StatusCodes.Status400BadRequest, "A required argument was null"),
            ArgumentException => (StatusCodes.Status400BadRequest, "An invalid argument was provided"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "The operation is invalid in the current state"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Access is unauthorized"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "The requested resource was not found"),
            NotSupportedException => (StatusCodes.Status501NotImplemented, "The requested operation is not supported"),
            TimeoutException => (StatusCodes.Status504GatewayTimeout, "The operation timed out"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.")
        };
    }
}
