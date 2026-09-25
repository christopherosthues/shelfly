using Shelfly.Api.Features.Admin.Endpoints;

namespace Shelfly.Api.Extensions;

public static class AdminEndpointExtensions
{
    extension(IEndpointRouteBuilder routes)
    {
        public IEndpointRouteBuilder MapAdminEndpoints()
        {
            RouteGroupBuilder group = routes.MapGroup("admin");

            group.MapGet("/config", GetConfigEndpoint.Handle);
            group.MapPut("/config", UpdateConfigEndpoint.Handle);
            group.MapPost("/config/reload", ReloadConfigEndpoint.Handle);

            return routes;
        }
    }
}
