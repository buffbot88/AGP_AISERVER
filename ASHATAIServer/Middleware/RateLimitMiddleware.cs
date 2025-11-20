using System.Collections.Concurrent;

namespace ASHATAIServer.Middleware
{
    /// <summary>
    /// Middleware for rate limiting requests per IP and per API key
    /// </summary>
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitMiddleware> _logger;
        private readonly int _requestsPerMinute;
        private readonly int _requestsPerHour;
        private readonly bool _enabled;
        
        // Store request counts: Key = IP or API key, Value = (minute count, hour count, minute timestamp, hour timestamp)
        private static readonly ConcurrentDictionary<string, RateLimitEntry> _requestCounts = new();
        
        // Cleanup timer to remove old entries
        private static readonly Timer _cleanupTimer;

        static RateLimitMiddleware()
        {
            // Cleanup expired entries every 5 minutes
            _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        public RateLimitMiddleware(
            RequestDelegate next, 
            ILogger<RateLimitMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            
            _enabled = configuration.GetValue<bool>("RateLimit:Enabled", true);
            _requestsPerMinute = configuration.GetValue<int>("RateLimit:RequestsPerMinute", 60);
            _requestsPerHour = configuration.GetValue<int>("RateLimit:RequestsPerHour", 1000);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_enabled)
            {
                await _next(context);
                return;
            }

            // Get identifier (API key if present, otherwise IP address)
            var identifier = GetIdentifier(context);
            
            // Check rate limits
            var now = DateTime.UtcNow;
            var entry = _requestCounts.GetOrAdd(identifier, _ => new RateLimitEntry());

            bool isRateLimited = false;
            string? rateLimitMessage = null;
            string? retryAfter = null;

            lock (entry)
            {
                // Reset counters if time windows have passed
                if ((now - entry.MinuteTimestamp).TotalMinutes >= 1)
                {
                    entry.MinuteCount = 0;
                    entry.MinuteTimestamp = now;
                }

                if ((now - entry.HourTimestamp).TotalHours >= 1)
                {
                    entry.HourCount = 0;
                    entry.HourTimestamp = now;
                }

                // Check if limits exceeded
                if (entry.MinuteCount >= _requestsPerMinute)
                {
                    isRateLimited = true;
                    rateLimitMessage = $"Rate limit exceeded. Maximum {_requestsPerMinute} requests per minute allowed.";
                    retryAfter = "60";
                }
                else if (entry.HourCount >= _requestsPerHour)
                {
                    isRateLimited = true;
                    var minutesUntilReset = (int)Math.Ceiling(60 - (now - entry.HourTimestamp).TotalMinutes);
                    rateLimitMessage = $"Rate limit exceeded. Maximum {_requestsPerHour} requests per hour allowed.";
                    retryAfter = (minutesUntilReset * 60).ToString();
                }
                else
                {
                    // Increment counters
                    entry.MinuteCount++;
                    entry.HourCount++;
                    entry.LastRequestTime = now;
                }
            }

            // Handle rate limit outside the lock
            if (isRateLimited)
            {
                _logger.LogWarning("Rate limit exceeded for: {Identifier}", SanitizeIdentifier(identifier));
                context.Response.StatusCode = 429; // Too Many Requests
                context.Response.ContentType = "application/json";
                context.Response.Headers["Retry-After"] = retryAfter!;
                await context.Response.WriteAsJsonAsync(new 
                { 
                    success = false, 
                    message = rateLimitMessage,
                    retryAfter = retryAfter + " seconds"
                });
                return;
            }

            // Add rate limit headers
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-RateLimit-Limit-Minute"] = _requestsPerMinute.ToString();
                context.Response.Headers["X-RateLimit-Limit-Hour"] = _requestsPerHour.ToString();
                context.Response.Headers["X-RateLimit-Remaining-Minute"] = Math.Max(0, _requestsPerMinute - entry.MinuteCount).ToString();
                context.Response.Headers["X-RateLimit-Remaining-Hour"] = Math.Max(0, _requestsPerHour - entry.HourCount).ToString();
                return Task.CompletedTask;
            });

            await _next(context);
        }

        private string GetIdentifier(HttpContext context)
        {
            // Try to get API key first (more specific than IP)
            if (context.Items.TryGetValue("ApiKeyUserId", out var userId) && userId != null)
            {
                return $"user_{userId}";
            }

            // Try to get API key from headers
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var authValue = authHeader.ToString();
                if (authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var apiKey = authValue["Bearer ".Length..].Trim();
                    if (!string.IsNullOrEmpty(apiKey))
                    {
                        return $"key_{apiKey[..Math.Min(16, apiKey.Length)]}"; // Use first 16 chars for identifier
                    }
                }
            }

            if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
            {
                var apiKey = apiKeyHeader.ToString();
                if (!string.IsNullOrEmpty(apiKey))
                {
                    return $"key_{apiKey[..Math.Min(16, apiKey.Length)]}";
                }
            }

            // Fall back to IP address
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            // Check for forwarded IP (reverse proxy scenario)
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                var forwarded = forwardedFor.ToString().Split(',')[0].Trim();
                if (!string.IsNullOrEmpty(forwarded))
                {
                    ip = forwarded;
                }
            }

            return $"ip_{ip}";
        }

        private static string SanitizeIdentifier(string identifier)
        {
            // Remove sensitive parts from API keys for logging
            if (identifier.StartsWith("key_"))
            {
                return "key_***";
            }
            return identifier;
        }

        private static void CleanupExpiredEntries(object? state)
        {
            var now = DateTime.UtcNow;
            var expiredKeys = new List<string>();

            foreach (var kvp in _requestCounts)
            {
                // Remove entries that haven't been used in the last hour
                if ((now - kvp.Value.LastRequestTime).TotalHours > 1)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in expiredKeys)
            {
                _requestCounts.TryRemove(key, out _);
            }
        }

        private class RateLimitEntry
        {
            public int MinuteCount { get; set; }
            public int HourCount { get; set; }
            public DateTime MinuteTimestamp { get; set; } = DateTime.UtcNow;
            public DateTime HourTimestamp { get; set; } = DateTime.UtcNow;
            public DateTime LastRequestTime { get; set; } = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Extension methods for registering the rate limit middleware
    /// </summary>
    public static class RateLimitMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitMiddleware>();
        }
    }
}
