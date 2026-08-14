using System.CommandLine;
using Microsoft.Extensions.Logging;
using Shelfly.AdminConsole.Enums;
using Shelfly.Configuration;
using Shelfly.AdminConsole.Services;

namespace Shelfly.AdminConsole.Commands;

public class ViewCommand : Command
{
    private readonly ILogger<ViewCommand> _logger;
    private readonly ConfigService _configService;
    private readonly Option<ConfigType[]> _configOption;

    public ViewCommand(ILogger<ViewCommand> logger, ConfigService configService)
        : base("view", "View current configuration")
    {
        _logger = logger;
        _configService = configService;
        _configOption = new("--config")
        {
            Required = true,
            Description = "Configuration type to view: All, Keycloak, PostgreSQL, MongoDb (repeatable)",
        };

        Options.Add(_configOption);
        SetAction(ViewConfigurationsAsync);
    }

    private async Task<int> ViewConfigurationsAsync(ParseResult parseResult)
    {
        ConfigType[]? configTypes = parseResult.GetValue(_configOption);

        // Default to All if no --config option provided
        if (configTypes == null || configTypes.Length == 0)
        {
            configTypes = [ConfigType.All];
        }

        // Check for All precedence
        bool showAll = configTypes.Contains(ConfigType.All);

        if (showAll)
        {
            await PrintAllConfigurations();
        }
        else
        {
            await PrintConfigurations(configTypes);
        }

        return 0;
    }

    private async Task PrintAllConfigurations()
    {
        await ViewKeycloakAsync();
        await ViewPostgreSqlAsync();
        await ViewAuthRulesAsync();
    }

    private async Task PrintConfigurations(ConfigType[] configTypes)
    {
        foreach (ConfigType type in configTypes.Distinct())
        {
            switch (type)
            {
                case ConfigType.Keycloak:
                    await ViewKeycloakAsync();
                    break;
                case ConfigType.PostgreSQL:
                    await ViewPostgreSqlAsync();
                    break;
                case ConfigType.MongoDb:
                    await ViewAuthRulesAsync();
                    break;
            }
        }
    }

    private async Task ViewKeycloakAsync()
    {
        KeycloakConfiguration? config = await _configService.LoadByIdAsync<KeycloakConfiguration>(KeycloakConfiguration.DefaultId);
        if (config != null)
        {
            _logger.LogInformation("Keycloak configuration: IssuerUrl={IssuerUrl}, Audience={Audience}",
                config.IssuerUrl, config.Audience);
        }
        else
        {
            _logger.LogInformation("Keycloak configuration: not configured");
        }
    }

    private async Task ViewPostgreSqlAsync()
    {
        PostgreSQLConfiguration? config = await _configService.LoadByIdAsync<PostgreSQLConfiguration>(PostgreSQLConfiguration.DefaultId);
        if (config != null)
        {
            _logger.LogInformation("PostgreSQL configuration: ConnectionString={ConnectionString}",
                config.ConnectionString);
        }
        else
        {
            _logger.LogInformation("PostgreSQL configuration: not configured");
        }
    }

    private async Task ViewAuthRulesAsync()
    {
        AuthorizationRule? authRules = await _configService.LoadByIdAsync<AuthorizationRule>(AuthorizationRule.DefaultId);
        if (authRules != null)
        {
            foreach (Rule rule in authRules.Rules)
            {
                _logger.LogInformation("Rule: Endpoint={EndpointPattern}, Roles=[{Roles}]",
                    rule.EndpointPattern, string.Join(", ", rule.RequiredRoles));
            }
        }
        else
        {
            _logger.LogInformation("Authorization rules: not configured");
        }
    }
}
