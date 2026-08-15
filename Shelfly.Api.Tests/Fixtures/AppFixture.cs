using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.Keycloak;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;

namespace Shelfly.Api.Tests.Fixtures;

public class AppFixture : IAsyncDisposable
{
    public WebApplicationFactory<Program> Factory { get; private set; } = new();
    public PostgreSqlContainer? PostgreSQLContainer { get; private set; }
    public MongoDbContainer? MongoDBContainer { get; private set; }
    public KeycloakContainer? KeycloakContainer { get; private set; }

    public async Task InitializeAsync()
    {
        PostgreSQLContainer = new PostgreSqlBuilder("shelfly_postgres")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithDatabase("shelfly_test")
            .Build();

        await PostgreSQLContainer.StartAsync();

        MongoDBContainer = new MongoDbBuilder("shelfly_mongodb")
            .WithUsername("admin")
            .WithPassword("password")
            .Build();

        await MongoDBContainer.StartAsync();

        KeycloakContainer = new KeycloakBuilder("shelfly_keycloak")
            .WithUsername("admin")
            .WithPassword("admin")
            .Build();

        await KeycloakContainer.StartAsync();

        Factory = new WebApplicationFactory<Program>();
    }

    public async ValueTask DisposeAsync()
    {
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        if (PostgreSQLContainer is not null)
        {
            await PostgreSQLContainer.StopAsync();
        }

        if (MongoDBContainer is not null)
        {
            await MongoDBContainer.StopAsync();
        }

        if (KeycloakContainer is not null)
        {
            await KeycloakContainer.StopAsync();
        }
    }
}
