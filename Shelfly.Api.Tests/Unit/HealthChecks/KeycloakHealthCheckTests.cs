using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shelfly.Api.Features.HealthChecks.Checks;

namespace Shelfly.Api.Tests.Unit.HealthChecks;

public class KeycloakHealthCheckTests
{
    [Test]
    public async Task CheckHealthAsync_ReturnsHealthy_WhenRealmEndpointResponds()
    {
        HttpResponseMessage mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        HttpClient mockClient = Substitute.For<HttpClient>();
        mockClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(mockResponse));

        KeycloakHealthCheck check = new KeycloakHealthCheck(mockClient, "test-realm");
        HealthCheckContext context = new HealthCheckContext { Registration = new HealthCheckRegistration("keycloak", _ => check, null, Array.Empty<string>()) };

        HealthCheckResult result = await check.CheckHealthAsync(context);

        result.Status.ShouldBe(HealthStatus.Healthy);
    }

    [Test]
    public async Task CheckHealthAsync_ReturnsUnhealthy_WhenRealmEndpointFails()
    {
        HttpResponseMessage mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable);
        HttpClient mockClient = Substitute.For<HttpClient>();
        mockClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(mockResponse));

        KeycloakHealthCheck check = new KeycloakHealthCheck(mockClient, "test-realm");
        HealthCheckContext context = new HealthCheckContext { Registration = new HealthCheckRegistration("keycloak", _ => check, null, []) };

        HealthCheckResult result = await check.CheckHealthAsync(context);

        result.Status.ShouldBe(HealthStatus.Unhealthy);
    }

    [Test]
    public async Task CheckHealthAsync_ReturnsUnhealthy_WhenExceptionThrown()
    {
        HttpClient mockClient = Substitute.For<HttpClient>();
        mockClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
        mockClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Throws(new System.Net.WebException("Connection refused"));

        KeycloakHealthCheck check = new KeycloakHealthCheck(mockClient, "test-realm");
        HealthCheckContext context = new HealthCheckContext { Registration = new HealthCheckRegistration("keycloak", _ => check, null, []) };

        HealthCheckResult result = await check.CheckHealthAsync(context);

        result.Status.ShouldBe(HealthStatus.Unhealthy);
    }
}
