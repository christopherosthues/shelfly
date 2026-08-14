using Microsoft.Extensions.Logging;
using Shelfly.Configuration;
using Shelfly.AdminConsole.Services;
using Shelfly.AdminConsole.Validation;

namespace Shelfly.AdminConsole.Commands;

internal class SetAuthRulesCommand(ILogger<SetAuthRulesCommand> logger, ConfigService configService)
    : SetConfigCommand<AuthorizationRule, AuthRulesValidator>(
        "auth-rules",
        "Set authorization rules from JSON file",
        logger,
        configService,
        AuthorizationRule.DefaultId);
