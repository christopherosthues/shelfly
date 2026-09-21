using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shelfly.Api.Extensions;
using Shelfly.Api.Features.Auth.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.AddOpenTelemetry();

// Add services to the container.
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

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

// Map authentication endpoints
app.MapAuthEndpoints();

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
