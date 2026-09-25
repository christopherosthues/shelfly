using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shelfly.Api.Extensions;
using Shelfly.Api.Features.Admin.Services;
using Shelfly.Api.Features.Auth.Services;
using Shelfly.Api.Features.Books.Services;
using Shelfly.Api.Features.Bookmarks.Services;
using Shelfly.Api.Features.HealthChecks.Checks;
using Shelfly.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.AddOpenTelemetry();

// Add services to the container.
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

string mongoConnectionString = MongoDbOptionsExtensions.BuildMongoConnectionString(builder.Configuration);

builder.Configuration.AddMongoDbConfiguration(mongoConnectionString);

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

// Register EF Core DbContext with PostgreSQL (will use fallback config initially)
builder.Services.AddShelflyDbContext(builder.Configuration);

// Feature services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();

builder.Services.AddProblemDetails();

// Dynamic options (MongoDB-backed configuration with change token support)
builder.Services.AddMongoDbOptions(builder.Configuration, mongoConnectionString);

// OpenTelemetry instrumentation for authentication and health check endpoints
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("shelfly-api"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddSource("shelfly-api")
        .AddSource("shelfly-health-checks")
        .AddOtlpExporter())
    .WithMetrics(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddOtlpExporter());

WebApplication app = builder.Build();

// Load dynamic configuration from MongoDB (seeds defaults if empty)
using (IServiceScope scope = app.Services.CreateScope())
{
    DynamicOptionsManager optionsManager = scope.ServiceProvider.GetRequiredService<DynamicOptionsManager>();
    await optionsManager.LoadAsync(CancellationToken.None);

    // Rebuild PostgreSQL connection string with MongoDB-backed config
    PostgreSqlConfig postgreSqlConfig = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<PostgreSqlConfig>>().CurrentValue;
    string postgresConnectionString = PostgreSqlOptionsExtensions.BuildPostgresConnectionString(postgreSqlConfig, builder.Configuration);

    // Register health checks with the resolved connection string
    builder.Services.AddHealthChecks()
        .AddCheck<LivenessHealthCheck>("liveness", tags: ["live"])
        .Add(new HealthCheckRegistration(
            "postgresql",
            _ => new PostgreSQLHealthCheck(postgresConnectionString),
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready"],
            timeout: TimeSpan.FromSeconds(3)))
        .Add(new HealthCheckRegistration(
            "mongodb",
            _ => new MongoDbHealthCheck(mongoConnectionString),
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready"],
            timeout: TimeSpan.FromSeconds(3)))
        .Add(new HealthCheckRegistration(
            "keycloak",
            sp =>
            {
                string keycloakUrl = builder.Configuration.GetValue<string>("Keycloak:BaseUrl")
                                     ?? throw new InvalidOperationException("Keycloak:BaseUrl not configured");
                string realm = builder.Configuration.GetValue<string>("Keycloak:Realm")
                               ?? "master";
                return new KeycloakHealthCheck(new HttpClient { BaseAddress = new Uri(keycloakUrl) }, realm);
            },
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready"],
            timeout: TimeSpan.FromSeconds(3)));
}

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
