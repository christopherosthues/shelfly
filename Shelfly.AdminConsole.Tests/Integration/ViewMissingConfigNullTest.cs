using System.Text.Json;
using Shouldly;
using Shelfly.Configuration;

namespace Shelfly.AdminConsole.Tests.Integration;

public class ViewMissingConfigNullTest : IntegrationTestBase
{
    [Test]
    public async Task GivenMissingKeycloakConfig_WhenSerializedToJson_ThenOutputsNull()
    {
        // Arrange — simulate missing config (null result from LoadByIdAsync)
        KeycloakConfiguration? config = null;

        // Act
        string json = JsonSerializer.Serialize(config);

        // Assert — should be literal "null" JSON
        json.ShouldBe("null");
    }

    [Test]
    public async Task GivenMissingPostgreSqlConfig_WhenSerializedToJson_ThenOutputsNull()
    {
        // Arrange
        PostgreSqlConfiguration? config = null;

        // Act
        string json = JsonSerializer.Serialize(config);

        // Assert
        json.ShouldBe("null");
    }

    [Test]
    public async Task GivenMissingAuthRules_WhenSerializedToJson_ThenOutputsNull()
    {
        // Arrange
        AuthorizationRule? config = null;

        // Act
        string json = JsonSerializer.Serialize(config);

        // Assert
        json.ShouldBe("null");
    }

    [Test]
    public async Task GivenAllConfigsMissing_WhenCreatingCombinedOutput_ThenAllSectionsAreNull()
    {
        // Arrange — simulate all configs missing from MongoDB
        KeycloakConfiguration? keycloak = null;
        PostgreSqlConfiguration? postgresql = null;
        AuthorizationRule? authRules = null;

        Dictionary<string, object?> combined = new()
        {
            ["keycloak"] = keycloak,
            ["postgresql"] = postgresql,
            ["authRules"] = authRules
        };

        // Act
        string json = JsonSerializer.Serialize(combined);

        // Assert — verify all sections are null in the JSON output
        using JsonDocument doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("keycloak").ValueKind.ShouldBe(JsonValueKind.Null);
        doc.RootElement.GetProperty("postgresql").ValueKind.ShouldBe(JsonValueKind.Null);
        doc.RootElement.GetProperty("authRules").ValueKind.ShouldBe(JsonValueKind.Null);
    }
}
