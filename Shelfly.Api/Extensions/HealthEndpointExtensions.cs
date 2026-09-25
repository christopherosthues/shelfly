using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shelfly.Api.Features.HealthChecks.DTOs;
using Shelfly.Api.Features.HealthChecks.Services;

namespace Shelfly.Api.Extensions;

/// <summary>
/// Extension methods for mapping health check endpoints.
/// </summary>
public static class HealthEndpointExtensions
{
    extension(IEndpointRouteBuilder routes)
    {
        /// <summary>
        /// Maps the liveness health check endpoint at /v1/health/live.
        /// </summary>
        public IEndpointRouteBuilder MapLiveHealthChecks()
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
        public IEndpointRouteBuilder MapReadyHealthChecks()
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
            HealthCheckResponseDto response = HealthCheckResultService.CreateResponse(report);

            context.Response.ContentType = "application/json";
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            await JsonSerializer.SerializeAsync(context.Response.Body, response, options);
        }
    }
}
