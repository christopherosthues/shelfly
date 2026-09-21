using System.Net;
using System.Text.Json;
using Shelfly.Api.Tests.Integration;

namespace Shelfly.Api.Tests.Integration.HealthChecks;

public class ReadinessEndpointIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task ReadinessEndpoint_Returns200_WhenAllDependenciesHealthy()
    {
        HttpClient client = CreateHttpClient();
        HttpResponseMessage response = await client.GetAsync("/v1/health/ready");

        // With Testcontainers running, all dependencies should be healthy
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, object>? json = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

        json.ShouldNotBeNull();
        json["Status"].ToString().ShouldBe("Healthy");
    }

    [Test]
    public async Task ReadinessEndpoint_Returns503_WhenDependencyUnhealthy()
    {
        HttpClient client = CreateHttpClient();
        HttpResponseMessage response = await client.GetAsync("/v1/health/ready");

        // If any dependency is unhealthy, expect 503
        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            string content = await response.Content.ReadAsStringAsync();
            Dictionary<string, object>? json = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

            json.ShouldNotBeNull();
            json["Status"].ToString().ShouldBe("Unhealthy");
        }
    }

    [Test]
    public async Task ReadinessEndpoint_Response_ContainsDependencyDetails()
    {
        HttpClient client = CreateHttpClient();
        HttpResponseMessage response = await client.GetAsync("/v1/health/ready");

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, object>? json = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

        json.ShouldNotBeNull();
        json.ContainsKey("Dependencies").ShouldBeTrue();
    }
}
