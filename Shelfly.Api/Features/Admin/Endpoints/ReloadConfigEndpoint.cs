using Microsoft.Extensions.Options;
using Shelfly.Api.Features.Admin.Services;
using Shelfly.Configuration;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Admin.Endpoints;

public static class ReloadConfigEndpoint
{
    public static async Task<IResult> Handle(
        DynamicOptionsManager optionsManager,
        IOptionsMonitor<ServerDynamicConfiguration> options,
        CancellationToken cancellationToken)
    {
        await optionsManager.LoadAsync(cancellationToken);

        return Ok(options.CurrentValue);
    }
}
