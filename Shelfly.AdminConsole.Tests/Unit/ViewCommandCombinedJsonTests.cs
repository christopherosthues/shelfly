using System.Text.Json;
using Shouldly;
using Shelfly.Configuration;

namespace Shelfly.AdminConsole.Tests.Unit;

public class ViewCommandCombinedJsonTests
{
    [Test]
    public void GivenAllConfigsPresent_WhenCreatingCombinedOutput_ThenHasCorrectKeys()
    {
        // Arrange
        KeycloakConfiguration keycloakConfig = KeycloakConfiguration.Create(
            "https://keycloak.example.com/realms/shelfly",
            "shelfly-api",
            "https://keycloak.example.com/protocol/openid-connect/certs",
            "admin-client-id",
            "admin-client-secret");

        PostgreSqlConfiguration postgresqlConfig = PostgreSqlConfiguration.Create(
            "Host=localhost;Port=5432;Database=shelfly;Username=admin;Password=password");

        AuthorizationRule authRules = AuthorizationRule.Create([
            new("GET:/api/books", ["user"]),
            new("POST:/api/bookmarks/*", ["user", "admin"])
        ]);

        // Act — simulate combined JSON structure
        Dictionary<string, object?> combined = new()
        {
            ["keycloak"] = keycloakConfig,
            ["postgresql"] = postgresqlConfig,
            ["authRules"] = authRules
        };

        string json = JsonSerializer.Serialize(combined);

        // Assert — verify JSON structure has correct keys and values
        using JsonDocument doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("keycloak", out JsonElement keycloak).ShouldBeTrue();
        doc.RootElement.TryGetProperty("postgresql", out JsonElement postgresql).ShouldBeTrue();
        doc.RootElement.TryGetProperty("authRules", out JsonElement authRulesElem).ShouldBeTrue();

        // Verify each section is a valid object (not null)
        keycloak.ValueKind.ShouldBe(JsonValueKind.Object);
        postgresql.ValueKind.ShouldBe(JsonValueKind.Object);
        authRulesElem.ValueKind.ShouldBe(JsonValueKind.Object);

        // Verify nested field access works
        keycloak.GetProperty("IssuerUrl").GetString().ShouldBe("https://keycloak.example.com/realms/shelfly");
        postgresql.GetProperty("ConnectionString").ToString().ShouldContain("shelfly");
    }

    [Test]
    public void GivenPartialConfigsMissing_WhenCreatingCombinedOutput_ThenMissingSectionsAreNull()
    {
        // Arrange — only Keycloak config present
        Dictionary<string, object?> combined = new()
        {
            ["keycloak"] = KeycloakConfiguration.Create(
                "https://keycloak.example.com/realms/shelfly",
                "shelfly-api",
                "https://keycloak.example.com/protocol/openid-connect/certs",
                "admin-client-id",
                "admin-client-secret"),
            ["postgresql"] = null,
            ["authRules"] = null
        };

        // Act
        string json = JsonSerializer.Serialize(combined);

        // Assert — verify structure with null sections
        using JsonDocument doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("keycloak", out JsonElement keycloak).ShouldBeTrue();
        doc.RootElement.TryGetProperty("postgresql", out JsonElement postgresql).ShouldBeTrue();
        doc.RootElement.TryGetProperty("authRules", out JsonElement authRulesElem).ShouldBeTrue();

        // Keycloak should be an object, others should be null
        keycloak.ValueKind.ShouldBe(JsonValueKind.Object);
        postgresql.ValueKind.ShouldBe(JsonValueKind.Null);
        authRulesElem.ValueKind.ShouldBe(JsonValueKind.Null);
    }
}
