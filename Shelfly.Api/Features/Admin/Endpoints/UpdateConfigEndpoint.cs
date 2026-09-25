using Microsoft.Extensions.Options;
using Shelfly.Api.Features.Admin.Services;
using Shelfly.Configuration;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Admin.Endpoints;

public static class UpdateConfigEndpoint
{
    public static async Task<IResult> Handle(
        DynamicOptionsManager optionsManager,
        IOptionsMonitor<ServerDynamicConfiguration> options,
        ServerDynamicConfiguration request,
        CancellationToken cancellationToken)
    {
        await optionsManager.SaveAsync(request, cancellationToken);

        return Ok(options.CurrentValue);
    }
}
