using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shelfly.Api.Features.HealthChecks.DTOs;

namespace Shelfly.Api.Extensions;

/// <summary>
/// Extension methods for mapping health check endpoints.
/// </summary>
public static class HealthEndpointExtensions
{
    /// <summary>
    /// Maps the liveness health check endpoint at /v1/health/live.
    /// </summary>
    public static IEndpointRouteBuilder MapLiveHealthChecks(this IEndpointRouteBuilder routes)
    {
        routes.MapHealthChecks("/v1/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteHealthCheckResponse,
            ResultStatusCodes = new Dictionary<HealthStatus, int>
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        return routes;
    }

    /// <summary>
    /// Maps the readiness health check endpoint at /v1/health/ready.
    /// </summary>
    public static IEndpointRouteBuilder MapReadyHealthChecks(this IEndpointRouteBuilder routes)
    {
        routes.MapHealthChecks("/v1/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponse,
            ResultStatusCodes = new Dictionary<HealthStatus, int>
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        return routes;
    }

    /// <summary>
    /// Custom response writer for structured JSON health check responses.
    /// </summary>
    private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        IEnumerable<DependencyStatusDto> dependencies = report.Entries.Select(entry => new DependencyStatusDto(
            entry.Key,
            entry.Value.Status == HealthStatus.Healthy ? "Healthy" : "Unhealthy",
            entry.Value.Status == HealthStatus.Unhealthy ? CategorizeFailure(entry.Value.Exception) : null,
            entry.Value.Duration));

        HealthCheckResponseDto response = new HealthCheckResponseDto(
            report.Status == HealthStatus.Healthy ? "Healthy" : "Unhealthy",
            dependencies,
            DateTimeOffset.UtcNow);

        context.Response.ContentType = "application/json";
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        await JsonSerializer.SerializeAsync(context.Response.Body, response, options);
    }

    /// <summary>
    /// Categorizes failure exceptions into actionable categories.
    /// </summary>
    private static string? CategorizeFailure(Exception? exception)
    {
        return exception switch
        {
            null => "timeout",
            _ when exception.Message.Contains("Timeout") || exception.GetType().Name.Contains("Timeout") => "timeout",
            _ when exception.Message.Contains("Connection") || exception.InnerException?.Message.Contains("Connection") == true => "connection refused",
            _ => "other"
        };
    }
}
