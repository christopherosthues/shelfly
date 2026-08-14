using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Authentication;
using Shelfly.Api.Authentication.Models;
using Shelfly.Api.Authentication.Validators;
using Shelfly.Api.Bookmarks;
using Shelfly.Api.Books;
using Shelfly.Api.Data;
using Shelfly.Api.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();

builder.Services.AddDbContext<ShelflyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<BookmarkService>();

// Auth validators
builder.Services.AddScoped<IValidator<RegistrationRequest>, RegistrationValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginValidator>();
builder.Services.AddScoped<IValidator<PasswordResetRequest>, PasswordResetValidator>();

// Configuration services
string mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb")
                               ?? throw new InvalidOperationException("MONGODB_CONNECTION_STRING not configured");

ILoggerFactory loggerFactory = LoggerFactory.Create(b => b.AddConsole());
ResilientMongoClient resilientMongoClient = new(
    loggerFactory.CreateLogger<ResilientMongoClient>());
resilientMongoClient.Initialize(mongoConnectionString, "shelfly-config");

builder.Services.AddSingleton(resilientMongoClient);
builder.Services.AddScoped<ConfigurationService>();
builder.Services.AddKeycloakAuthentication();
builder.Services.AddMemoryCache();

// Keycloak admin client for user management operations
builder.Services.AddHttpClient<KeycloakAdminClient>(client =>
{
    client.BaseAddress = new("http://todo.todo/");
});

WebApplication app = builder.Build();

// Seed default configuration if empty (using scoped service)
using IServiceScope scope = app.Services.CreateScope();
ConfigurationService configurationService = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
await configurationService.SeedDefaultsAsync();

// Load and apply Keycloak authentication configuration asynchronously
await app.LoadAndApplyKeycloakConfigAsync(configurationService);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map authentication endpoints
app.MapAuthEndpoints();
