using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shelfly.Api.Data;
using Shelfly.Api.Tests.Helpers;
using Testcontainers.Keycloak;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace Shelfly.Api.Tests.Integration;

public class ShelflyWebApplicationFactory : WebApplicationFactory<Program>, IAsyncInitializer
{
    // TODO: apply migrations
    // TODO: seed MongoDB
    // TODO: create realm.json and fill in test data

    private readonly KeycloakContainer _keycloakContainer = new KeycloakBuilder("keycloak/keycloak:26.7")
        .WithUsername(KeycloakConfig.TestUserName)
        .WithPassword(KeycloakConfig.TestPassword)
        .WithResourceMapping("./Import/", "/opt/keycloak/data/import")
        .WithCommand("--import-realm")
        .WithEnvironment(new Dictionary<string, string>
        {
            {"KC_BOOTSTRAP_ADMIN_USERNAME", "admin"},
            {"KC_BOOTSTRAP_ADMIN_PASSWORD", "admin"},
            {"KC_HTTP_ENABLED", "true"},
        })
        .Build();

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder("postgres:18.4")
        .Build();

    private readonly MongoDbContainer _mongoDbContainer =
        new MongoDbBuilder("mongodb/mongodb-community-server:8.3.8-ubuntu2204-slim").Build();

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync(CancellationToken.None);
        await _keycloakContainer.StartAsync(CancellationToken.None);
        await _mongoDbContainer.StartAsync(CancellationToken.None);
    }

    public override async ValueTask DisposeAsync()
    {
        // TODO Disposure of the containers here could be a problem based on the setup
        await _postgreSqlContainer.StopAsync(CancellationToken.None);
        await _postgreSqlContainer.DisposeAsync();

        await _keycloakContainer.StopAsync(CancellationToken.None);
        await _keycloakContainer.DisposeAsync();

        await _mongoDbContainer.StopAsync(CancellationToken.None);
        await _mongoDbContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            string? keycloakBaseAddress = _keycloakContainer.GetBaseAddress();
            configuration.AddInMemoryCollection(
                new Dictionary<string, string>
                {
                    { "Keycloak:Audience", "TestTracker" },
                    { "Keycloak:MetadataAddress", $"{keycloakBaseAddress}realms/{KeycloakConfig.Realm}/.well-known/openid-configuration" },
                    { "Keycloak:ValidIssuer", $"{keycloakBaseAddress}realms/{KeycloakConfig.Realm}" },
                    { "Keycloak:AuthorizationUrl", $"{keycloakBaseAddress}realms/{KeycloakConfig.Realm}/protocol/openid-connect/auth" },
                    { "Keycloak:ClientId", KeycloakConfig.ClientId },
                    { "Keycloak:ClientSecret", KeycloakConfig.TestClientSecret },
                    { "Keycloak:Authority", $"{keycloakBaseAddress}realms/{KeycloakConfig.Realm}" },
                    { "Keycloak:RegistrationUrl", $"{keycloakBaseAddress}admin/realms/{KeycloakConfig.Realm}/users" },
                    { "Keycloak:TokenUrl", $"{keycloakBaseAddress}realms/{KeycloakConfig.Realm}/protocol/openid-connect/token" },
                }!
            );
        });

        builder.ConfigureTestServices(services =>
        {
            ServiceDescriptor? dbContextDescriptor =
                services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ShelflyDbContext>));
            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<ShelflyDbContext>((_, options) =>
            {
                options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
            });
        });

        string? mongoConnectionString = _mongoDbContainer?.GetConnectionString();
        if (mongoConnectionString is not null)
        {
            builder.ConfigureAppConfiguration(configuration =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:MongoDb"] = mongoConnectionString
                }));
        }

        base.ConfigureWebHost(builder);
    }

    public string? KeycloakBaseAddress()
    {
        return _keycloakContainer.GetBaseAddress();
    }

    public string MongoDbConnectionString()
    {
        return _mongoDbContainer.GetConnectionString();
    }
}