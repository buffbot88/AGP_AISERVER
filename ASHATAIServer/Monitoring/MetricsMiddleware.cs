using System.Diagnostics;

namespace ASHATAIServer.Monitoring;

/// <summary>
/// Middleware to collect metrics for all HTTP requests
/// </summary>
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MetricsService _metricsService;
    private readonly ILogger<MetricsMiddleware> _logger;

    public MetricsMiddleware(RequestDelegate next, MetricsService metricsService, ILogger<MetricsMiddleware> logger)
    {
        _next = next;
        _metricsService = metricsService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        _metricsService.IncrementActiveConnections();

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _metricsService.RecordError(ex.GetType().Name, context.Request.Path);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _metricsService.DecrementActiveConnections();

            var durationSeconds = stopwatch.Elapsed.TotalSeconds;
            _metricsService.RecordRequest(
                context.Request.Path,
                context.Request.Method,
                context.Response.StatusCode,
                durationSeconds);
        }
    }
}

/// <summary>
/// Extension methods for registering metrics middleware
/// </summary>
public static class MetricsMiddlewareExtensions
{
    public static IApplicationBuilder UseMetrics(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MetricsMiddleware>();
    }
}
