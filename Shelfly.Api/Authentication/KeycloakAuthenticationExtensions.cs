using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Shelfly.Api.Configuration;
using Shelfly.Configuration;

namespace Shelfly.Api.Authentication;

public static class KeycloakAuthenticationExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        return services;
    }

    public static async Task<KeycloakConfiguration?> LoadAndApplyKeycloakConfigAsync(
        this IHost host,
        ConfigurationService configurationService)
    {
        KeycloakConfiguration? keycloakConfig = await configurationService.LoadKeycloakConfigAsync();

        if (keycloakConfig is null)
        {
            throw new InvalidOperationException("Keycloak configuration not found in MongoDB");
        }

        using IServiceScope scope = host.Services.CreateScope();
        IOptionsMonitor<JwtBearerOptions> monitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();

        JwtBearerOptions options = monitor.Get(JwtBearerDefaults.AuthenticationScheme);
        options.Authority = keycloakConfig.IssuerUrl;
        options.Audience = keycloakConfig.Audience;
        options.TokenValidationParameters = new()
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        options.Events = new()
        {
            OnAuthenticationFailed = context =>
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }
        };

        return keycloakConfig;
    }
}
