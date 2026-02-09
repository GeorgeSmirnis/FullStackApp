using System.Text;

namespace UserManagementAPI.Middleware;

/// <summary>
/// Configuration and constants for authentication middleware
/// </summary>
public static class AuthenticationConstants
{
    /// <summary>
    /// Bearer token prefix for Authorization header
    /// </summary>
    public const string BearerPrefix = "Bearer ";

    /// <summary>
    /// Header name for API key alternative authentication
    /// </summary>
    public const string ApiKeyHeader = "X-API-Key";

    /// <summary>
    /// Default API key for demo purposes (should be loaded from secure configuration in production)
    /// </summary>
    public const string DefaultApiKey = "demo-api-key-2026";
}

/// <summary>
/// Middleware for validating authentication tokens
/// Supports both Bearer tokens and API key authentication
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private readonly List<string> _excludedPaths;
    private readonly string _apiKey;

    /// <summary>
    /// Initializes the AuthenticationMiddleware with excluded paths and API key configuration
    /// </summary>
    public AuthenticationMiddleware(
        RequestDelegate next,
        ILogger<AuthenticationMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Paths that don't require authentication
        _excludedPaths = new List<string>
        {
            "/health",
            "/swagger",
            "/swagger/index.html",
            "/swagger/v1/swagger.json",
            "/openapi.json"
        };

        // Load API key from configuration, use default if not found
        _apiKey = configuration?["Authentication:ApiKey"] ?? AuthenticationConstants.DefaultApiKey;
    }

    /// <summary>
    /// Invokes the middleware to validate authentication
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Check if current path requires authentication
        if (IsPathExcluded(context.Request.Path))
        {
            _logger.LogDebug("Path {Path} is excluded from authentication", context.Request.Path);
            await _next(context);
            return;
        }

        // Validate authentication token
        var (isValid, tokenType) = ValidateToken(context);

        if (!isValid)
        {
            _logger.LogWarning(
                "Unauthorized access attempt to {Path} from {ClientIP}",
                context.Request.Path,
                context.Connection.RemoteIpAddress
            );

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = new
                {
                    message = "Unauthorized - Invalid or missing authentication token",
                    statusCode = StatusCodes.Status401Unauthorized,
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path
                }
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
            return;
        }

        _logger.LogInformation(
            "Authorized request to {Path} using {TokenType} authentication",
            context.Request.Path,
            tokenType
        );

        // Call next middleware if authentication is valid
        await _next(context);
    }

    /// <summary>
    /// Validates whether the token is valid (Bearer token or API key)
    /// </summary>
    private (bool isValid, string tokenType) ValidateToken(HttpContext context)
    {
        var request = context.Request;

        // Check for Bearer token in Authorization header
        if (request.Headers.ContainsKey("Authorization"))
        {
            var authHeader = request.Headers["Authorization"].ToString();

            if (authHeader.StartsWith(AuthenticationConstants.BearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring(AuthenticationConstants.BearerPrefix.Length);
                var isValid = ValidateBearerToken(token);
                return (isValid, "Bearer Token");
            }
        }

        // Check for API key in custom header
        if (request.Headers.ContainsKey(AuthenticationConstants.ApiKeyHeader))
        {
            var apiKey = request.Headers[AuthenticationConstants.ApiKeyHeader].ToString();
            var isValid = ValidateApiKey(apiKey);
            return (isValid, "API Key");
        }

        // No authentication provided
        return (false, "None");
    }

    /// <summary>
    /// Validates Bearer token format and content
    /// In production, this would validate against a token service (JWT, OAuth, etc.)
    /// </summary>
    private bool ValidateBearerToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        // For demo purposes, accept any non-empty token
        // In production, this would validate JWT or call an auth service
        // Example: validate against JWT secret, check expiration, etc.

        // Demo validation: token should not be all whitespace and should have minimum length
        if (token.Length < 10)
        {
            _logger.LogWarning("Bearer token too short: length {Length}", token.Length);
            return false;
        }

        _logger.LogDebug("Bearer token validation successful");
        return true;
    }

    /// <summary>
    /// Validates API key against configured key
    /// </summary>
    private bool ValidateApiKey(string providedKey)
    {
        if (string.IsNullOrWhiteSpace(providedKey))
        {
            return false;
        }

        var isValid = providedKey.Equals(_apiKey, StringComparison.Ordinal);

        if (!isValid)
        {
            _logger.LogWarning("Invalid API key provided");
        }
        else
        {
            _logger.LogDebug("API key validation successful");
        }

        return isValid;
    }

    /// <summary>
    /// Checks if the request path is excluded from authentication
    /// </summary>
    private bool IsPathExcluded(PathString path)
    {
        return _excludedPaths.Any(excludedPath =>
            path.StartsWithSegments(excludedPath, StringComparison.OrdinalIgnoreCase)
        );
    }
}
