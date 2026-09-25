using Microsoft.Extensions.Options;
using Shelfly.Configuration;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Admin.Endpoints;

public static class GetConfigEndpoint
{
    public static IResult Handle(IOptionsMonitor<ServerDynamicConfiguration> options) => Ok(options.CurrentValue);
}
