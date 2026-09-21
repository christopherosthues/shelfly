using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Http.Results;
using Shelfly.Api.Features.Auth.Services;

namespace Shelfly.Api.Features.Auth.Endpoints;

public static class RefreshEndpoint
{
    public static async Task<IResult> Handle(
        KeycloakAdminClient keycloakAdmin,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        string issuer = configuration.GetValue<string>("Keycloak:Issuer") ?? "";

        // TODO: Implement token refresh logic
        return Ok();
    }
}
