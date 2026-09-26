using Microsoft.Extensions.Options;
using Shelfly.Configuration;

namespace Shelfly.Api.Features.AdminUI.Services;

/// <summary>
/// Service to manage server dynamic configuration.
/// Uses IOptionsMonitor for reading and DynamicOptionsManager for persistence.
/// </summary>
public class ConfigService(IOptionsMonitor<ServerDynamicConfiguration> optionsMonitor, 
    Shelfly.Api.Features.Admin.Services.DynamicOptionsManager optionsManager)
{
    /// <summary>
    /// Gets the current server configuration.
    /// </summary>
    public ServerDynamicConfiguration GetCurrent() => optionsMonitor.CurrentValue;

    /// <summary>
    /// Saves a new configuration to MongoDB and returns the updated configuration.
    /// </summary>
    public async Task<ServerDynamicConfiguration> SaveAsync(ServerDynamicConfiguration config, CancellationToken cancellationToken = default)
    {
        await optionsManager.SaveAsync(config, cancellationToken);
        return optionsMonitor.CurrentValue;
    }

    /// <summary>
    /// Reloads configuration from MongoDB.
    /// </summary>
    public async Task<ServerDynamicConfiguration> ReloadAsync(CancellationToken cancellationToken = default)
    {
        await optionsManager.LoadAsync(cancellationToken);
        return optionsMonitor.CurrentValue;
    }
}
