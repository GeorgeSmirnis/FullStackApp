namespace UserManagementAPI.Middleware;

/// <summary>
/// Middleware for logging all HTTP requests and responses for auditing purposes
/// Captures: HTTP method, request path, query parameters, response status code, execution time
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    /// <summary>
    /// Initializes the RequestLoggingMiddleware
    /// </summary>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware to log request and response information
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Capture request start time for duration calculation
        var startTime = DateTime.UtcNow;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Log incoming request
        LogRequest(context, startTime);

        // Capture original response body stream
        var originalResponseBody = context.Response.Body;

        try
        {
            // Use a memory stream to capture the response body
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                // Call next middleware
                await _next(context);

                // Log outgoing response
                stopwatch.Stop();
                LogResponse(context, stopwatch.ElapsedMilliseconds);

                // Copy captured response back to original stream
                await responseBody.CopyToAsync(originalResponseBody);
            }
        }
        finally
        {
            // Ensure original response body is restored
            context.Response.Body = originalResponseBody;
        }
    }

    /// <summary>
    /// Logs incoming HTTP request details
    /// </summary>
    private void LogRequest(HttpContext context, DateTime timestamp)
    {
        var request = context.Request;
        var method = request.Method;
        var path = request.Path;
        var queryString = request.QueryString;
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var requestInfo = new
        {
            Timestamp = timestamp,
            Method = method,
            Path = path.Value,
            QueryString = queryString.Value ?? "(none)",
            ClientIP = clientIp,
            ContentType = request.ContentType ?? "(none)"
        };

        _logger.LogInformation(
            "HTTP Request: {Method} {Path}{QueryString} from {ClientIP} at {Timestamp}",
            method,
            path,
            queryString,
            clientIp,
            timestamp
        );
    }

    /// <summary>
    /// Logs outgoing HTTP response details
    /// </summary>
    private void LogResponse(HttpContext context, long elapsedMilliseconds)
    {
        var response = context.Response;
        var statusCode = response.StatusCode;
        var request = context.Request;

        var responseInfo = new
        {
            Timestamp = DateTime.UtcNow,
            Method = request.Method,
            Path = request.Path.Value,
            StatusCode = statusCode,
            ElapsedMs = elapsedMilliseconds,
            ContentType = response.ContentType ?? "(none)"
        };

        // Log with appropriate level based on status code
        if (statusCode >= 500)
        {
            _logger.LogError(
                "HTTP Response: {Method} {Path} returned {StatusCode} in {ElapsedMs}ms",
                request.Method,
                request.Path,
                statusCode,
                elapsedMilliseconds
            );
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning(
                "HTTP Response: {Method} {Path} returned {StatusCode} in {ElapsedMs}ms",
                request.Method,
                request.Path,
                statusCode,
                elapsedMilliseconds
            );
        }
        else
        {
            _logger.LogInformation(
                "HTTP Response: {Method} {Path} returned {StatusCode} in {ElapsedMs}ms",
                request.Method,
                request.Path,
                statusCode,
                elapsedMilliseconds
            );
        }
    }
}
