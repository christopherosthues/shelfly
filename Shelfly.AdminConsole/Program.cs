using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Shelfly.AdminConsole.Commands;
using Shelfly.AdminConsole.Services;

IHost host = Host.CreateDefaultBuilder()
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddNLog();
    })
    .ConfigureServices(services => services.AddSingleton<ConfigService>()
        .AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddNLog();
        }))
    .Build();

IServiceScope scope = host.Services.CreateScope();
IServiceProvider serviceProvider = scope.ServiceProvider;

string? connectionString = ParseConnectionString(args);

RootCommand rootCommand = new("Admin MongoDB Configuration Management Console");

rootCommand.Options.Add(new Option<string>("--connection-string")
{
    Required = true,
    Description = "MongoDB connection string"
});

ConfigService configService = serviceProvider.GetRequiredService<ConfigService>();
if (connectionString != null)
{
    configService.Initialize(connectionString);
}

Command setCommand = new("set", "Set configuration from JSON file");

rootCommand.Add(setCommand);

// Set subcommands
setCommand.Subcommands.Add(new SetKeycloakCommand(
    serviceProvider.GetRequiredService<ILogger<SetKeycloakCommand>>(), configService));
setCommand.Subcommands.Add(new SetPostgreSQLCommand(
    serviceProvider.GetRequiredService<ILogger<SetPostgreSQLCommand>>(), configService));
setCommand.Subcommands.Add(new SetAuthRulesCommand(
    serviceProvider.GetRequiredService<ILogger<SetAuthRulesCommand>>(), configService));

// View command (unified)
rootCommand.Add(new ViewCommand(
    serviceProvider.GetRequiredService<ILogger<ViewCommand>>(), configService));

// Export command (separate from view)
Command exportCommand = new("export", "Export configuration to file");
exportCommand.Subcommands.Add(new ExportToFileHandler(
    serviceProvider.GetRequiredService<ILogger<ExportToFileHandler>>(), configService));
rootCommand.Add(exportCommand);

return await rootCommand.Parse(args).InvokeAsync();

static string? ParseConnectionString(string[] args)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (args[i] == "--connection-string")
        {
            return args[i + 1];
        }
    }

    return null;
}
