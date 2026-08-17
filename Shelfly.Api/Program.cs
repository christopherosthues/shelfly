
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Configuration services
string mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb")
                               ?? throw new InvalidOperationException("MONGODB_CONNECTION_STRING not configured");

ILoggerFactory loggerFactory = LoggerFactory.Create(b => b.AddConsole());

WebApplication app = builder.Build();

// Seed default configuration if empty (using scoped service)

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
