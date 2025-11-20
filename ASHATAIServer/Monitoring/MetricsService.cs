using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ASHATAIServer.Monitoring;

/// <summary>
/// Metrics provider for ASHATAIServer using OpenTelemetry and Prometheus
/// </summary>
public class MetricsService
{
    private readonly Meter _meter;
    private readonly Counter<long> _requestCounter;
    private readonly Counter<long> _errorCounter;
    private readonly Histogram<double> _requestDuration;
    private readonly Counter<long> _projectGenerationCounter;
    private readonly Histogram<double> _projectGenerationDuration;
    private readonly ObservableGauge<int> _activeConnections;
    private int _currentActiveConnections;

    public MetricsService()
    {
        _meter = new Meter("ASHATAIServer", "1.0.0");

        // Request metrics
        _requestCounter = _meter.CreateCounter<long>(
            "ashat_requests_total",
            description: "Total number of HTTP requests");

        _errorCounter = _meter.CreateCounter<long>(
            "ashat_errors_total",
            description: "Total number of errors");

        _requestDuration = _meter.CreateHistogram<double>(
            "ashat_request_duration_seconds",
            unit: "s",
            description: "Request duration in seconds");

        // Project generation metrics
        _projectGenerationCounter = _meter.CreateCounter<long>(
            "ashat_project_generations_total",
            description: "Total number of project generations");

        _projectGenerationDuration = _meter.CreateHistogram<double>(
            "ashat_project_generation_duration_seconds",
            unit: "s",
            description: "Project generation duration in seconds");

        // Active connections
        _activeConnections = _meter.CreateObservableGauge(
            "ashat_active_connections",
            () => _currentActiveConnections,
            description: "Number of active connections");
    }

    public void RecordRequest(string endpoint, string method, int statusCode, double durationSeconds)
    {
        _requestCounter.Add(1, 
            new KeyValuePair<string, object?>("endpoint", endpoint),
            new KeyValuePair<string, object?>("method", method),
            new KeyValuePair<string, object?>("status", statusCode));

        _requestDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("endpoint", endpoint),
            new KeyValuePair<string, object?>("method", method));
    }

    public void RecordError(string errorType, string endpoint)
    {
        _errorCounter.Add(1,
            new KeyValuePair<string, object?>("error_type", errorType),
            new KeyValuePair<string, object?>("endpoint", endpoint));
    }

    public void RecordProjectGeneration(string projectType, string language, double durationSeconds, bool success)
    {
        _projectGenerationCounter.Add(1,
            new KeyValuePair<string, object?>("project_type", projectType),
            new KeyValuePair<string, object?>("language", language),
            new KeyValuePair<string, object?>("success", success));

        _projectGenerationDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("project_type", projectType),
            new KeyValuePair<string, object?>("language", language));
    }

    public void IncrementActiveConnections() => Interlocked.Increment(ref _currentActiveConnections);
    public void DecrementActiveConnections() => Interlocked.Decrement(ref _currentActiveConnections);
}
