using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Http.Results;
using Shelfly.Api.Features.Auth.Services;

namespace Shelfly.Api.Features.Auth.Endpoints;

public static class ResetPasswordEndpoint
{
    public static async Task<IResult> Handle(
        KeycloakAdminClient keycloakAdmin,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        string realm = configuration.GetValue<string>("Keycloak:Realm") ?? "master";

        // TODO: Implement password reset logic
        return Ok();
    }
}
