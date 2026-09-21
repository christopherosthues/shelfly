using System.Text.Json.Serialization;

namespace Shelfly.Api.Features.HealthChecks.DTOs;

/// <summary>
/// Represents a single dependency health check result.
/// </summary>
public record DependencyStatusDto(
    string Name,
    string Status,
    string? FailureCategory,
    TimeSpan Duration)
{
    /// <summary>
    /// Dependency identifier: "PostgreSQL", "MongoDB", or "Keycloak"
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = Name;

    /// <summary>
    /// Check result: "Healthy" or "Unhealthy"
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = Status;

    /// <summary>
    /// Failure classification when unhealthy: "timeout", "connection refused", or null when healthy
    /// </summary>
    [JsonPropertyName("failureCategory")]
    public string? FailureCategory { get; init; } = FailureCategory;

    /// <summary>
    /// Time elapsed for this check
    /// </summary>
    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; init; } = Duration;
}
