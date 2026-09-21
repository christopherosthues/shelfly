using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shelfly.Api.Extensions;
using Shelfly.Api.Features.Auth.Services;
using Shelfly.Api.Features.HealthChecks.DTOs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.AddOpenTelemetry();

// Add services to the container.
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Health check services
builder.Services.AddHealthChecks()
    .AddCheck("liveness", () => HealthCheckResult.Healthy("Process is running"), tags: ["live"]);

// Configuration services
string mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb")
                               ?? throw new InvalidOperationException("MONGODB_CONNECTION_STRING not configured");

ILoggerFactory loggerFactory = LoggerFactory.Create(b => b.AddConsole());

// Authentication feature services
builder.Services.AddHttpClient("Keycloak", c =>
{
    string keycloakUrl = builder.Configuration.GetValue<string>("Keycloak:BaseUrl")
                         ?? throw new InvalidOperationException("Keycloak:BaseUrl not configured");
    c.BaseAddress = new Uri(keycloakUrl);
});

string realm = builder.Configuration.GetValue<string>("Keycloak:Realm")
               ?? "master";

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddScoped<KeycloakAdminClient>();
builder.Services.AddScoped<RateLimitService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddProblemDetails();

// MongoDB resilience with Polly retry policy
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    MongoClient client = new MongoClient(mongoConnectionString);
    return client.GetDatabase("shelfly");
});

// OpenTelemetry instrumentation for authentication endpoints
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("shelfly-api"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddSource("shelfly-api")
        .AddOtlpExporter())
    .WithMetrics(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddOtlpExporter());

WebApplication app = builder.Build();

// Seed default configuration if empty (using scoped service)
// app.Services.AddProblemDetails();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Health check response writer for structured JSON output
static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
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
    JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
    await System.Text.Json.JsonSerializer.SerializeAsync(context.Response.Body, response, options);
}

static string? CategorizeFailure(Exception? exception)
{
    return exception switch
    {
        null => "timeout",
        _ when exception.Message.Contains("Timeout") || exception.GetType().Name.Contains("Timeout") => "timeout",
        _ when exception.Message.Contains("Connection") || exception.InnerException?.Message.Contains("Connection") == true => "connection refused",
        _ => "other"
    };
}

// Map authentication endpoints
app.MapAuthEndpoints();

// Health check endpoints
app.MapHealthChecks("/v1/health/live", new HealthCheckOptions
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

app.MapHealthChecks("/v1/health/ready", new HealthCheckOptions
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

// Global error handling middleware for Keycloak connectivity failures
app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke(context);
    }
    catch (Exception ex) when (ex.Message.Contains("Keycloak") || (ex.InnerException?.Message.Contains("Keycloak") ?? false))
    {
        context.Response.StatusCode = StatusCodes.Status502BadGateway;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = 502,
            Title = "Bad Gateway",
            Detail = "Keycloak service temporarily unavailable",
            Type = "https://tools.ietf.org/html/rfc7807#section-2.1"
        });
    }
});
