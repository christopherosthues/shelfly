using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shelfly.Api.Constants;
using Shelfly.Api.Extensions;
using Shelfly.Api.Extensions.Providers;
using Shelfly.Api.Features.Auth.Services;
using Shelfly.Api.Features.Books.Services;
using Shelfly.Api.Features.Bookmarks.Services;
using Shelfly.Api.Features.HealthChecks.Checks;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.AddOpenTelemetry();

// Add services to the container.
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

string mongoConnectionString = MongoDbOptionsExtensions.BuildMongoConnectionString(builder.Configuration);

builder.Configuration.AddMongoDbConfiguration(mongoConnectionString);

// Authentication feature services
builder.Services.AddHttpClient("Keycloak");

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddScoped<KeycloakAdminClient>();
builder.Services.AddScoped<RateLimitService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Register EF Core DbContext with PostgreSQL (will use fallback config initially)
builder.Services.AddShelflyDbContext(builder.Configuration);

// Feature services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<PostgreSqlHealthCheck>();

builder.Services.AddProblemDetails();

// Dynamic options (MongoDB-backed configuration with change token support)
builder.Services.AddMongoDbOptions(builder.Configuration, mongoConnectionString);

builder.Services.AddHealthChecks()
    .AddCheck<LivenessHealthCheck>(HealthCheckNames.Liveness, tags: [HealthCheckTags.Live])
    .Add(new HealthCheckRegistration(
        HealthCheckNames.PostgreSql,
        sp => sp.GetRequiredService<PostgreSqlHealthCheck>(),
        failureStatus: HealthStatus.Unhealthy,
        tags: [HealthCheckTags.Ready],
        timeout: TimeSpan.FromSeconds(3)))
    .Add(new HealthCheckRegistration(
        HealthCheckNames.MongoDb,
        _ => new MongoDbHealthCheck(mongoConnectionString),
        failureStatus: HealthStatus.Unhealthy,
        tags: [HealthCheckTags.Ready],
        timeout: TimeSpan.FromSeconds(3)))
    .Add(new HealthCheckRegistration(
        HealthCheckNames.Keycloak,
        sp => sp.GetRequiredService<KeycloakHealthCheck>(),
        failureStatus: HealthStatus.Unhealthy,
        tags: [HealthCheckTags.Ready],
        timeout: TimeSpan.FromSeconds(3)));

// OpenTelemetry instrumentation for authentication and health check endpoints
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(ActivitySources.Api))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddSource(ActivitySources.Api)
        .AddSource(ActivitySources.HealthChecks)
        .AddSource(ActivitySources.ConfigProvider)
        .AddOtlpExporter())
    .WithMetrics(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddMeter(ActivitySources.ConfigProvider)
        .AddOtlpExporter());

WebApplication app = builder.Build();

// Wire OTel metrics and logging to the MongoDB configuration provider
MongoDbConfigurationProvider.SetLoggerFactory(app.Services.GetRequiredService<ILoggerFactory>());
Meter configMeter = app.Services.GetRequiredService<IMeterFactory>().Create(ActivitySources.ConfigProvider);
MongoDbConfigurationProvider.SetMeter(configMeter);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map authentication endpoints
app.MapAuthEndpoints();

// Map admin configuration endpoints
app.MapAdminEndpoints();

// Map books and bookmarks endpoints
app.MapBooksEndpoints();
app.MapBookmarksEndpoints();

// Map health check endpoints
app.MapLiveHealthChecks();
app.MapReadyHealthChecks();

// Global error handling middleware for Keycloak connectivity failures
app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke(context);
    }
    catch (Exception ex) when (ex.Message.Contains(HealthCheckNames.Keycloak) || (ex.InnerException?.Message.Contains(HealthCheckNames.Keycloak) ?? false))
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
