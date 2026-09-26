using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Shelfly.Api.Constants;
using Shelfly.Api.Features.AdminUI;

namespace Shelfly.Api.Extensions;

/// <summary>
/// Extension methods for mapping Blazor admin UI endpoints.
/// </summary>
public static class AdminUiEndpointExtensions
{
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

    /// <summary>
    /// Maps the Blazor Server admin UI with authentication and authorization requirements.
    /// Called from Program.cs within a MapGroup block.
    /// </summary>
    public static WebApplication MapBlazorAdminUi(this WebApplication app)
    {
        RouteGroupBuilder blazorGroup = app.MapGroup("/admin");
        blazorGroup.MapControllers();
        blazorGroup.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .RequireAuthorization("AdminOnly");

        return app;
    }
}
