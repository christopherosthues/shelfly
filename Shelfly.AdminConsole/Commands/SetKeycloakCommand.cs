using Microsoft.Extensions.Logging;
using Shelfly.Configuration;
using Shelfly.AdminConsole.Services;
using Shelfly.AdminConsole.Validation;

namespace Shelfly.AdminConsole.Commands;

internal class SetKeycloakCommand(ILogger<SetKeycloakCommand> logger, ConfigService configService)
    : SetConfigCommand<KeycloakConfiguration, KeycloakConfigValidator>(
        "keycloak",
        "Set Keycloak configuration from JSON file",
        logger,
        configService,
        KeycloakConfiguration.DefaultId);
