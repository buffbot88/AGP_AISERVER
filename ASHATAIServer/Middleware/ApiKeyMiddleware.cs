using ASHATAIServer.Services.Auth;

namespace ASHATAIServer.Middleware
{
    /// <summary>
    /// Middleware to validate API keys for protected endpoints
    /// </summary>
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyMiddleware> _logger;
        private readonly List<string> _protectedPaths;
        private readonly List<string> _exemptPaths;

        public ApiKeyMiddleware(
            RequestDelegate next, 
            ILogger<ApiKeyMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            
            // Paths that require API key authentication
            _protectedPaths = configuration.GetSection("ApiKey:ProtectedPaths")
                .Get<List<string>>() ?? new List<string>();
            
            // Paths that are exempt from API key requirement
            _exemptPaths = configuration.GetSection("ApiKey:ExemptPaths")
                .Get<List<string>>() ?? new List<string>
                {
                    "/api/auth/login",
                    "/api/auth/register",
                    "/api/auth/validate",
                    "/api/ai/health"
                };
        }

        public async Task InvokeAsync(HttpContext context, ApiKeyService apiKeyService)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            // Skip middleware if path is exempt
            if (_exemptPaths.Any(p => path.StartsWith(p.ToLowerInvariant())))
            {
                await _next(context);
                return;
            }

            // Check if path requires API key (if protected paths are configured)
            if (_protectedPaths.Count > 0 && !_protectedPaths.Any(p => path.StartsWith(p.ToLowerInvariant())))
            {
                await _next(context);
                return;
            }

            // Try to get API key from Authorization header (Bearer scheme) or X-API-Key header
            string? apiKey = null;
            
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var authValue = authHeader.ToString();
                if (authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    apiKey = authValue["Bearer ".Length..].Trim();
                }
            }
            
            if (string.IsNullOrEmpty(apiKey) && context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
            {
                apiKey = apiKeyHeader.ToString();
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("API key missing for protected endpoint: {Path}", path);
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new 
                { 
                    success = false, 
                    message = "API key required. Provide via Authorization: Bearer <key> or X-API-Key: <key> header" 
                });
                return;
            }

            // Validate API key
            var (isValid, userId, scopes, message) = await apiKeyService.ValidateApiKeyAsync(apiKey);

            if (!isValid)
            {
                _logger.LogWarning("Invalid API key attempted for path: {Path}", path);
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new 
                { 
                    success = false, 
                    message = message ?? "Invalid or expired API key" 
                });
                return;
            }

            // Store API key info in HttpContext for use by controllers
            context.Items["ApiKeyUserId"] = userId;
            context.Items["ApiKeyScopes"] = scopes;

            _logger.LogDebug("API key validated for path: {Path}, UserId: {UserId}", path, userId);

            await _next(context);
        }
    }

    /// <summary>
    /// Extension methods for registering the API key middleware
    /// </summary>
    public static class ApiKeyMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyMiddleware>();
        }
    }
}
