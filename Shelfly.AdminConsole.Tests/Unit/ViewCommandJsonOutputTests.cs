using System.Text.Json;
using Shouldly;
using Shelfly.Configuration;

namespace Shelfly.AdminConsole.Tests.Unit;

public class ViewCommandJsonOutputTests
{
    [Test]
    public void GivenKeycloakConfig_WhenSerializedToJson_ThenContainsAllFields()
    {
        // Arrange
        KeycloakConfiguration config = KeycloakConfiguration.Create(
            "https://keycloak.example.com/realms/shelfly",
            "shelfly-api",
            "https://keycloak.example.com/protocol/openid-connect/certs",
            "admin-client-id",
            "admin-client-secret");

        // Act
        string json = JsonSerializer.Serialize(config);

        // Assert — JSON should be valid and contain all fields
        using JsonDocument doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("Id", out JsonElement id).ShouldBeTrue();
        doc.RootElement.TryGetProperty("IssuerUrl", out JsonElement issuerUrl).ShouldBeTrue();
        doc.RootElement.TryGetProperty("Audience", out JsonElement audience).ShouldBeTrue();
        doc.RootElement.TryGetProperty("JwksEndpoint", out JsonElement jwksEndpoint).ShouldBeTrue();
        doc.RootElement.TryGetProperty("AdminClientId", out JsonElement adminClientId).ShouldBeTrue();
        doc.RootElement.TryGetProperty("AdminClientSecret", out JsonElement adminClientSecret).ShouldBeTrue();

        id.GetString().ShouldBe("keycloak");
        issuerUrl.GetString().ShouldBe("https://keycloak.example.com/realms/shelfly");
        audience.GetString().ShouldBe("shelfly-api");
    }

    [Test]
    public void GivenPostgreSqlConfig_WhenSerializedToJson_ThenContainsAllFields()
    {
        // Arrange
        PostgreSqlConfiguration config = PostgreSqlConfiguration.Create(
            "Host=localhost;Port=5432;Database=shelfly;Username=admin;Password=password");

        // Act
        string json = JsonSerializer.Serialize(config);

        // Assert
        using JsonDocument doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("Id", out JsonElement id).ShouldBeTrue();
        doc.RootElement.TryGetProperty("ConnectionString", out JsonElement connectionString).ShouldBeTrue();

        id.GetString().ShouldBe("postgresql");
    }

    [Test]
    public void GivenAuthRules_WhenSerializedToJson_ThenContainsAllFields()
    {
        // Arrange
        AuthorizationRule authRules = AuthorizationRule.Create([
            new("GET:/api/books", ["user"]),
            new("POST:/api/bookmarks/*", ["user", "admin"])
        ]);

        // Act
        string json = JsonSerializer.Serialize(authRules);

        // Assert
        using JsonDocument doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("Id", out JsonElement id).ShouldBeTrue();
        doc.RootElement.TryGetProperty("Rules", out JsonElement rules).ShouldBeTrue();

        id.GetString().ShouldBe("auth-rules");
        rules.ValueKind.ShouldBe(JsonValueKind.Array);
        rules.GetArrayLength().ShouldBe(2);
    }

    [Test]
    public void GivenNullConfig_WhenSerializedToJson_ThenOutputsNull()
    {
        // Arrange
        KeycloakConfiguration? config = null;

        // Act
        string json = JsonSerializer.Serialize(config);

        // Assert — should be literal "null"
        json.ShouldBe("null");
    }
}
