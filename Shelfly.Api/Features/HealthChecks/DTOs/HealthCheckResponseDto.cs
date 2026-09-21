using System.Text.Json.Serialization;

namespace Shelfly.Api.Features.HealthChecks.DTOs;

/// <summary>
/// Represents the structured health check response returned by both liveness and readiness endpoints.
/// </summary>
public record HealthCheckResponseDto(
    string Status,
    IEnumerable<DependencyStatusDto> Dependencies,
    DateTimeOffset Timestamp)
{
    /// <summary>
    /// Overall health status: "Healthy", "Degraded", or "Unhealthy"
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = Status;

    /// <summary>
    /// Individual dependency check results (readiness only)
    /// </summary>
    [JsonPropertyName("dependencies")]
    public IEnumerable<DependencyStatusDto> Dependencies { get; init; } = Dependencies;

    /// <summary>
    /// Response generation time
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; init; } = Timestamp;
}
