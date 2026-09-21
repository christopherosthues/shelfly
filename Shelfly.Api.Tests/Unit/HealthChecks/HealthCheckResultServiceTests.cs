using System.Collections.ObjectModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shelfly.Api.Features.HealthChecks.DTOs;
using Shelfly.Api.Features.HealthChecks.Services;

namespace Shelfly.Api.Tests.Unit.HealthChecks;

public class HealthCheckResultServiceTests
{
    [Test]
    public void MapOverallStatus_MapsHealthStatusCorrectly()
    {
        HealthCheckResultService.MapOverallStatus(HealthStatus.Healthy).ShouldBe("Healthy");
        HealthCheckResultService.MapOverallStatus(HealthStatus.Degraded).ShouldBe("Degraded");
        HealthCheckResultService.MapOverallStatus(HealthStatus.Unhealthy).ShouldBe("Unhealthy");
    }

    [Test]
    public void CategorizeFailure_ReturnsTimeout_WhenExceptionIsNull()
    {
        HealthCheckResultService.CategorizeFailure(null).ShouldBe("timeout");
    }

    [Test]
    public void CategorizeFailure_ReturnsTimeout_WhenMessageContainsTimeout()
    {
        Exception exception = new System.Exception("Operation Timeout");
        HealthCheckResultService.CategorizeFailure(exception).ShouldBe("timeout");
    }

    [Test]
    public void CategorizeFailure_ReturnsConnectionRefused_WhenMessageContainsConnection()
    {
        Exception exception = new System.Exception("Connection refused");
        HealthCheckResultService.CategorizeFailure(exception).ShouldBe("connection refused");
    }

    [Test]
    public void CategorizeFailure_ReturnsOther_ForGenericException()
    {
        Exception exception = new System.Exception("Unknown error");
        HealthCheckResultService.CategorizeFailure(exception).ShouldBe("other");
    }

    [Test]
    public void CreateResponse_CreatesValidHealthCheckResponseDto()
    {
        HealthReport report = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["liveness"] = new HealthReportEntry(HealthStatus.Healthy, "Liveness check passed", TimeSpan.Zero, null, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>())),
            }, TimeSpan.Zero);

        HealthCheckResponseDto response = HealthCheckResultService.CreateResponse(report);

        response.Status.ShouldBe("Healthy");
        response.Dependencies.ShouldContain(d => d.Name == "liveness" && d.Status == "Healthy");
    }
}
