using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Shelfly.Api.Tests.Integration;

namespace Shelfly.Api.Tests.Integration.HealthChecks;

public class HealthEndpointIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task LivenessEndpoint_Returns200WithHealthyStatus()
    {
        HttpClient client = CreateHttpClient();
        HttpResponseMessage response = await client.GetAsync("/v1/health/live");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, object>? json = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

        json.ShouldNotBeNull();
        json["Status"].ToString().ShouldBe("Healthy");
    }

    [Test]
    public async Task LivenessEndpoint_ResponseTime_Under200ms()
    {
        HttpClient client = CreateHttpClient();

        Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        HttpResponseMessage response = await client.GetAsync("/v1/health/live");
        stopwatch.Stop();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(200);
    }
}
