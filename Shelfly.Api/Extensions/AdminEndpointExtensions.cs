using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Shelfly.Api.Constants;
using Shelfly.Api.Features.Admin.Endpoints;
using Shelfly.Api.Features.AdminUI;

namespace Shelfly.Api.Extensions;

public static class AdminEndpointExtensions
{
    extension(IEndpointRouteBuilder routes)
    {
        public IEndpointRouteBuilder MapAdminEndpoints()
        {
            RouteGroupBuilder group = routes.MapGroup("admin").RequireAuthorization();

            group.MapGet("/config", GetConfigEndpoint.Handle);
            group.MapPut("/config", UpdateConfigEndpoint.Handle);
            group.MapPost("/config/reload", ReloadConfigEndpoint.Handle);

            group.MapControllers();
            group.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .RequireAuthorization("AdminOnly");

            return routes;
        }
    }

    public static IServiceCollection AddAdminUi(this IServiceCollection serviceCollection)
    {
        // Add services to the container.
        serviceCollection.AddAuthentication(options =>
        {
            options.DefaultScheme = OpenIdConnectDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        });

        serviceCollection.AddOptions()
            .Configure<OpenIdConnectOptions>(options =>
            {
                options.ClientId = "shelfly-admin";
                options.ResponseType = "code";
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
            });

        serviceCollection.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationSchemes.AdminOnly, policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "realm_access" && c.Value.Contains("admin"))));

        // Register Blazor Server components and MVC for admin UI hosting
        serviceCollection.AddRazorComponents()
            .AddInteractiveServerComponents();

        serviceCollection.AddControllersWithViews();

        return serviceCollection;
    }
}
