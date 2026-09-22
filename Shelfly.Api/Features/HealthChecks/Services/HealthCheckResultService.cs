using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shelfly.Api.Features.HealthChecks.DTOs;

namespace Shelfly.Api.Features.HealthChecks.Services;

/// <summary>
/// Aggregates health check results and categorizes failures for structured reporting.
/// </summary>
public static class HealthCheckResultService
{
    /// <summary>
    /// Converts a raw HealthReport into a structured response DTO with dependency details.
    /// </summary>
    public static HealthCheckResponseDto CreateResponse(HealthReport report)
    {
        return new HealthCheckResponseDto(
            MapOverallStatus(report.Status),
            CreateDependencies(report.Entries),
            DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Maps the raw health status to a user-friendly string.
    /// </summary>
    public static string MapOverallStatus(HealthStatus status)
    {
        return status switch
        {
            HealthStatus.Healthy => "Healthy",
            HealthStatus.Degraded => "Degraded",
            _ => "Unhealthy"
        };
    }

    /// <summary>
    /// Creates dependency status entries from health check report entries.
    /// </summary>
    public static IEnumerable<DependencyStatusDto> CreateDependencies(IReadOnlyDictionary<string, HealthReportEntry> entries)
    {
        return entries.Select(entry => new DependencyStatusDto(
            entry.Key,
            entry.Value.Status == HealthStatus.Healthy ? "Healthy" : "Unhealthy",
            entry.Value.Status switch
            {
                HealthStatus.Unhealthy => CategorizeFailure(entry.Value.Exception),
                _ => null
            },
            entry.Value.Duration));
    }

    /// <summary>
    /// Categorizes failure exceptions into actionable categories for monitoring and alerting.
    /// </summary>
    public static string? CategorizeFailure(Exception? exception)
    {
        return exception switch
        {
            null => "timeout",
            _ when IsTimeout(exception) => "timeout",
            _ when IsConnectionRefused(exception) => "connection refused",
            _ => "other"
        };
    }

    /// <summary>
    /// Detects timeout-related exceptions.
    /// </summary>
    private static bool IsTimeout(Exception ex)
    {
        return ex.Message.Contains("Timeout") || ex.GetType().Name.Contains("Timeout");
    }

    /// <summary>
    /// Detects connection-related exceptions.
    /// </summary>
    private static bool IsConnectionRefused(Exception ex)
    {
        return ex.Message.Contains("Connection") || (ex.InnerException?.Message.Contains("Connection") ?? false);
    }
}
