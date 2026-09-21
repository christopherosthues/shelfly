using MongoDB.Driver;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shelfly.Api.Extensions;
using Shelfly.Api.Features.Auth.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
        .AddSource("shelfly-api")
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
