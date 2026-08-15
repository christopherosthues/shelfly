using System.Text.Json;
using Shouldly;
using Shelfly.Configuration;

namespace Shelfly.AdminConsole.Tests.Unit;

public class GenerateCommandDefaultValuesTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [Test]
    public void GivenKeycloakConfig_WhenGeneratedWithDefaults_ThenContainsAllFieldsWithEmptyStrings()
    {
        // Arrange - create default Keycloak configuration using the same logic as GenerateCommand
        KeycloakConfiguration config = new(
            KeycloakConfiguration.DefaultId,
            "",
            "",
            "",
            "",
            "");

        // Act
        string json = JsonSerializer.Serialize(config, JsonOptions);

        // Assert — JSON should be valid and contain all fields with default values
        using JsonDocument doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("Id", out JsonElement id).ShouldBeTrue();
        doc.RootElement.TryGetProperty("IssuerUrl", out JsonElement issuerUrl).ShouldBeTrue();
        doc.RootElement.TryGetProperty("Audience", out JsonElement audience).ShouldBeTrue();
        doc.RootElement.TryGetProperty("JwksEndpoint", out JsonElement jwksEndpoint).ShouldBeTrue();
        doc.RootElement.TryGetProperty("AdminClientId", out JsonElement adminClientId).ShouldBeTrue();
        doc.RootElement.TryGetProperty("AdminClientSecret", out JsonElement adminClientSecret).ShouldBeTrue();

        id.GetString().ShouldBe(KeycloakConfiguration.DefaultId);
        issuerUrl.GetString().ShouldBe("");
        audience.GetString().ShouldBe("");
        jwksEndpoint.GetString().ShouldBe("");
        adminClientId.GetString().ShouldBe("");
        adminClientSecret.GetString().ShouldBe("");
    }

    [Test]
    public void GivenPostgreSqlConfig_WhenGeneratedWithDefaults_ThenContainsAllFieldsWithEmptyStrings()
    {
        // Arrange - create default PostgreSQL configuration using the same logic as GenerateCommand
        PostgreSqlConfiguration config = new(
            PostgreSqlConfiguration.DefaultId,
            "");

        // Act
        string json = JsonSerializer.Serialize(config, JsonOptions);

        // Assert — JSON should be valid and contain all fields with default values
        using JsonDocument doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("Id", out JsonElement id).ShouldBeTrue();
        doc.RootElement.TryGetProperty("ConnectionString", out JsonElement connectionString).ShouldBeTrue();

        id.GetString().ShouldBe(PostgreSqlConfiguration.DefaultId);
        connectionString.GetString().ShouldBe("");
    }

    [Test]
    public void GivenAuthRules_WhenGeneratedWithDefaults_ThenContainsEmptyRulesList()
    {
        // Arrange - create default AuthorizationRule using the same logic as GenerateCommand
        AuthorizationRule authRules = new(
            AuthorizationRule.DefaultId,
            []);

        // Act
        string json = JsonSerializer.Serialize(authRules, JsonOptions);

        // Assert — JSON should be valid and contain all fields with default values
        using JsonDocument doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("Id", out JsonElement id).ShouldBeTrue();
        doc.RootElement.TryGetProperty("Rules", out JsonElement rules).ShouldBeTrue();

        id.GetString().ShouldBe(AuthorizationRule.DefaultId);
        rules.ValueKind.ShouldBe(JsonValueKind.Array);
        rules.GetArrayLength().ShouldBe(0);
    }
}
