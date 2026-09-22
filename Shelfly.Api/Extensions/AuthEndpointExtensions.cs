using Shelfly.Api.Features.Auth.Endpoints;

namespace Shelfly.Api.Extensions;

public static class AuthEndpointExtensions
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("v1/auth");

        group.MapPost("/register", RegisterEndpoint.Handle);
        group.MapPost("/login", LoginEndpoint.Handle);
        group.MapPost("/refresh", RefreshEndpoint.Handle);
        group.MapPost("/reset-password", ResetPasswordEndpoint.Handle);

        return routes;
    }
}
