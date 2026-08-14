using Microsoft.Extensions.Logging;
using Shelfly.Configuration;
using Shelfly.AdminConsole.Services;
using Shelfly.AdminConsole.Validation;

namespace Shelfly.AdminConsole.Commands;

internal class SetPostgreSQLCommand(ILogger<SetPostgreSQLCommand> logger, ConfigService configService)
    : SetConfigCommand<PostgreSqlConfiguration, PostgreSqlConfigValidator>(
        "postgresql",
        "Set PostgreSQL configuration from JSON file",
        logger,
        configService,
        PostgreSqlConfiguration.DefaultId);
